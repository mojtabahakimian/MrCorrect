using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Prg_UI.Functions
{
    public static class PageManagement
    {
        public static Frame TheMainFame { get; set; }
        private static Stack<Page> navigationStack = new Stack<Page>();
        public static void OpenPage(Page _pg)
        {
            Page previousPage = (Page)TheMainFame.Content; // Get the current page
            navigationStack.Push(previousPage); // Push the current page onto the stack

            TheMainFame.Visibility = Visibility.Visible;
            TheMainFame.Navigate(_pg);
        }
        public static void DisplayPageManagement(KeyEventArgs e, Frame frme)
        {
            if (e.Key == Key.Escape && navigationStack.Count > 0)
            {
                Page previousPage = navigationStack.Pop(); // Pop the top page from the stack
                frme.Visibility = Visibility.Visible;
                frme.Content = previousPage; // Navigate to the previous page
                e.Handled = true;

                //DoubleAnimation fadeOutAnimation = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.1));
                //currentPage.BeginAnimation(Page.OpacityProperty, fadeOutAnimation);
            }
        }
    }
}
