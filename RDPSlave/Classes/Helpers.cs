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
        /// <returns></returns>
        public static string TryGetElementValue(this XElement parentEl, string elementName, string defaultValue = null)
        {
            var foundEl = parentEl.Element(elementName);

            if (foundEl != null)
            {
                return foundEl.Value;
            }

            return defaultValue;
        }
    }
}
