using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace CodeMonkeyPlayer
{
    // ActiveX does not expose WinForms drag events. Register an OLE file-drop target
    // on its native video windows, including windows recreated when media changes.
    internal sealed class VideoFileDropTarget : Component
    {
        private readonly Control player;
        private readonly DropTarget target;
        private readonly HashSet<IntPtr> registered = new HashSet<IntPtr>();
        private bool disposed;

        public VideoFileDropTarget(Control player, Action<string[]> onDrop)
        {
            this.player = player;
            target = new DropTarget(onDrop);
        }

        public void Refresh()
        {
            if (disposed || !player.IsHandleCreated) return;
            var windows = new HashSet<IntPtr> { player.Handle };
            EnumChildWindows(player.Handle, (window, data) => { windows.Add(window); return true; }, IntPtr.Zero);
            registered.RemoveWhere(window =>
            {
                if (windows.Contains(window)) return false;
                if (IsWindow(window)) RevokeDragDrop(window);
                return true;
            });
            foreach (IntPtr window in windows)
            {
                if (registered.Contains(window)) continue;
                int result = RegisterDragDrop(window, target);
                if (result == unchecked((int)0x80040101)) // DRAGDROP_E_ALREADYREGISTERED
                {
                    RevokeDragDrop(window);
                    result = RegisterDragDrop(window, target);
                }
                if (result == 0) registered.Add(window);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                foreach (IntPtr window in registered)
                    if (IsWindow(window)) RevokeDragDrop(window);
                registered.Clear();
            }
            base.Dispose(disposing);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PointL { public int X, Y; }

        [ComVisible(true), ComImport, Guid("00000122-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleDropTarget
        {
            [PreserveSig] int DragEnter([MarshalAs(UnmanagedType.Interface)] IDataObject data, uint keys, PointL point, ref uint effect);
            [PreserveSig] int DragOver(uint keys, PointL point, ref uint effect);
            [PreserveSig] int DragLeave();
            [PreserveSig] int Drop([MarshalAs(UnmanagedType.Interface)] IDataObject data, uint keys, PointL point, ref uint effect);
        }

        [ComVisible(true), ClassInterface(ClassInterfaceType.None)]
        public sealed class DropTarget : IOleDropTarget
        {
            private readonly Action<string[]> onDrop;
            private bool acceptsFiles;
            public DropTarget(Action<string[]> onDrop) { this.onDrop = onDrop; }

            private static FORMATETC FileFormat()
            {
                return new FORMATETC { cfFormat = 15, dwAspect = DVASPECT.DVASPECT_CONTENT, lindex = -1, tymed = TYMED.TYMED_HGLOBAL };
            }

            public int DragEnter(IDataObject data, uint keys, PointL point, ref uint effect)
            {
                var format = FileFormat();
                try { acceptsFiles = data != null && data.QueryGetData(ref format) == 0; }
                catch (COMException) { acceptsFiles = false; }
                effect = acceptsFiles ? effect & 1U : 0;
                return 0;
            }
            public int DragOver(uint keys, PointL point, ref uint effect)
            {
                effect = acceptsFiles ? effect & 1U : 0;
                return 0;
            }
            public int DragLeave() { acceptsFiles = false; return 0; }
            public int Drop(IDataObject data, uint keys, PointL point, ref uint effect)
            {
                uint allowed = effect;
                effect = 0;
                acceptsFiles = false;
                if ((allowed & 1) == 0 || data == null) return 0;
                var format = FileFormat();
                STGMEDIUM medium;
                try { data.GetData(ref format, out medium); }
                catch (COMException) { return 0; }
                string[] files;
                try
                {
                    uint count = DragQueryFile(medium.unionmember, uint.MaxValue, null, 0);
                    files = new string[count];
                    for (uint i = 0; i < count; i++)
                    {
                        uint length = DragQueryFile(medium.unionmember, i, null, 0);
                        var path = new StringBuilder((int)length + 1);
                        DragQueryFile(medium.unionmember, i, path, (uint)path.Capacity);
                        files[i] = path.ToString();
                    }
                }
                finally { ReleaseStgMedium(ref medium); }
                if (files.Length > 0) { onDrop(files); effect = 1; }
                return 0;
            }
        }

        private delegate bool EnumWindowCallback(IntPtr window, IntPtr data);
        [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr parent, EnumWindowCallback callback, IntPtr data);
        [DllImport("user32.dll")] private static extern bool IsWindow(IntPtr window);
        [DllImport("ole32.dll")] private static extern int RegisterDragDrop(IntPtr window, [MarshalAs(UnmanagedType.Interface)] IOleDropTarget target);
        [DllImport("ole32.dll")] private static extern int RevokeDragDrop(IntPtr window);
        [DllImport("ole32.dll")] private static extern void ReleaseStgMedium(ref STGMEDIUM medium);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)] private static extern uint DragQueryFile(IntPtr drop, uint index, StringBuilder path, uint size);
    }
}
