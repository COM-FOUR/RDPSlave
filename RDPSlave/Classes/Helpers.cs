using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RDPSlave
{
    /// <summary>
    /// conteins various methods to simplify handling of tasks
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// checks if an element exists in a XElement object and returns it, or returns defaultValue if provided
        /// </summary>
        /// <param name="parentEl">XElement to search in</param>
        /// <param name="elementName">name of element to search for</param>
        /// <param name="defaultValue">default value, if element is not found</param>
        /// <returns>return value</returns>
        public static string TryGetElementValue(this XElement parentEl, string elementName, string defaultValue = null)
        {
            var foundEl = parentEl.Element(elementName);

            if (foundEl != null)
            {
                return foundEl.Value;
            }

            return defaultValue;
        }
        /// <summary>
        /// sets the maximum number of JumplistItems displayed in WindowsTaskbar
        /// </summary>
        /// <param name="count">maximum number to be set</param>
        /// <returns>true, if succeeded</returns>
        public static bool SetMaxJumpListItems(int count)
        {
            bool result = false;

            try
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "JumpListItems_Maximum", count, RegistryValueKind.DWord);
                
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
    }
}
