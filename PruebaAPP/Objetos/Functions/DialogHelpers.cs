using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaAPP.Objetos.Functions
{
    public static class DialogHelpers
    {
        public static async Task<bool> DisplayMessage(string title, string message, string ok, string? cancel = null)
        {
            // Obtener la página principal de forma segura
            var mainPage = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0].Page : null;

            // Verificamos si hay botones
            if (mainPage == null)
                return false;

            // Mostramos cuadro de dialogo
            if (string.IsNullOrEmpty(cancel))
            {
                // Solo un botón
                await mainPage.DisplayAlert(title, message, ok);
                return true;
            }
            else
            {
                // Dos botones
                bool result = await mainPage.DisplayAlert(title, message, ok, cancel);
                return result;
            }
        }

        public static async Task<string> DisplayAction(string title, string cancel, string? destroy = null, string[] buttons = null!)
        {
            // Obtener la página principal de forma segura
            var mainPage = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0].Page : null;

            // Verificamos si hay botones
            if (mainPage == null)
                return string.Empty;

            // Mostramos cuadro de dialogo
            var result = await mainPage.DisplayActionSheet(title, cancel, destroy, buttons);
            return result;
        }

        public static async Task<string> DisplayPrompt(string title, string message, string accept, string cancel, string placeholder, int maxleght, Keyboard keyboard, string? initialValue = null)
        {
            // Obtener la página principal de forma segura
            var mainPage = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0].Page : null;
            
            // Verificamos si hay botones
            if (mainPage == null)
                return string.Empty;
            
            // Mostramos cuadro de dialogo
            var result = await mainPage.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxleght, keyboard, initialValue: initialValue);
            return result ?? string.Empty;
        }
    }
}
