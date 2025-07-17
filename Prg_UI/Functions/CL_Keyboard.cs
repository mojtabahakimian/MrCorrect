using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Functions
{
    public static class CL_Keyboard
    {
        // 1. PInvoke declarations
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ActivateKeyboardLayout(IntPtr hkl, uint Flags);

        private const uint KLF_ACTIVATE = 0x00000001;

        // 2. Known keyboard layouts (you can add more if needed)
        private static readonly Dictionary<string, string> KnownLayouts = new Dictionary<string, string>
        {
            { "English", "00000409" }, // U.S. English
            { "Farsi",   "00000429" }, // Farsi (Persian)
        };

        // 3. Cache for loaded HKLs (optional but can help performance if you switch layouts frequently)
        private static readonly Dictionary<string, IntPtr> LoadedLayoutsCache = new Dictionary<string, IntPtr>();

        public static void ChangeKeyboardLayout(string layoutKey)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(layoutKey) || !KnownLayouts.ContainsKey(layoutKey))
            {
                return;
            }

            string langID = KnownLayouts[layoutKey];

            try
            {
                // Check if we've already loaded this layout before
                if (LoadedLayoutsCache.TryGetValue(langID, out IntPtr cachedHkl) && cachedHkl != IntPtr.Zero)
                {
                    // Activate the cached layout
                    if (!ActivateKeyboardLayout(cachedHkl, KLF_ACTIVATE))
                    {
                        //ShowError($"Failed to activate previously loaded layout: {layoutKey}");
                    }
                }
                else
                {
                    // Load a new layout
                    IntPtr hkl = LoadKeyboardLayout(langID, KLF_ACTIVATE);

                    // Check if load was successful
                    if (hkl == IntPtr.Zero)
                    {
                        //int errorCode = Marshal.GetLastWin32Error();
                        //ShowError($"Failed to load layout: {layoutKey} (LangID: {langID}) ErrorCode: {errorCode}");
                        return;
                    }

                    // Try activating the loaded layout
                    bool activated = ActivateKeyboardLayout(hkl, KLF_ACTIVATE);
                    if (!activated)
                    {
                        //int errorCode = Marshal.GetLastWin32Error();
                        //ShowError($"Failed to activate layout: {layoutKey} (LangID: {langID}) ErrorCode: {errorCode}");
                        return;
                    }

                    // Store in cache for future use
                    LoadedLayoutsCache[langID] = hkl;
                }
            }
            catch (Exception ex)
            {
                // In some rare cases, PInvoke can throw exceptions, e.g. due to invalid parameters or system issues
                //ShowError($"Unexpected error while changing layout to {layoutKey}: {ex.Message}");
            }
        }
    }
}
