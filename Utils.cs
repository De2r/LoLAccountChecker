﻿#region License

// Copyright 2015 LoLAccountChecker
// 
// This file is part of LoLAccountChecker.
// 
// LoLAccountChecker is free software: you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// LoLAccountChecker is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License 
// along with LoLAccountChecker. If not, see http://www.gnu.org/licenses/.

#endregion

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LoLAccountChecker.Classes;

#endregion

namespace LoLAccountChecker
{
    internal partial class Utils
    {
        public static List<string[]> GetLogins(string file)
        {
            List<string[]> accounts = new List<string[]>();

            StreamReader sr = new StreamReader(file);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                {
                    continue;
                }

                accounts.Add(line.Split(':'));
            }

            return accounts;
        }

        public static void ExportLogins(string file, List<Account> accounts, bool exportErrors)
        {
            using (StreamWriter sw = new StreamWriter(file))
            {
                if (!exportErrors)
                {
                    accounts = accounts.Where(a => a.State == Account.Result.Success).ToList();
                }

                foreach (Account account in accounts)
                {
                    sw.WriteLine("{0}:{1}", account.Username, account.Password);
                }
            }
        }

        public static void ExportException(Exception e)
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string file = $"crash_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.txt";

            using (var sw = new StreamWriter(Path.Combine(dir, file)))
            {
                sw.WriteLine(e.ToString());
            }
        }

        public static async Task<string> GetHtmlResponse(string url, CookieContainer cookieContainer = null)
        {
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);

            if (cookieContainer != null)
            {
                wr.CookieContainer = cookieContainer;
            }

            try
            {
                string html;

                using (WebResponse resp = await wr.GetResponseAsync())
                using (Stream stream = resp.GetResponseStream())
                using (StreamReader sr = new StreamReader(stream))
                {
                    html = await sr.ReadToEndAsync();
                }

                return html;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse resp = (HttpWebResponse)response;
                    Console.WriteLine(resp);
                }

                return string.Empty;
            }
        }

        public static void AccountsDataDataGridSearchByLetterKey(object sender, KeyEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            Func<object, bool> itemCompareMethod = item =>
            {
                Account account = item as Account;

                if (account != null && account.Username.StartsWith(e.Key.ToString(), true, CultureInfo.CurrentCulture))
                {
                    return true;
                }

                return false;
            };

            DataGridSearchByLetterKey(dataGrid, e.Key, itemCompareMethod);
        }

        public static void AccountsDataGrid_RightClickCommand(object sender, DataGrid accountsDataGrid)
        {
            if (accountsDataGrid.SelectedItems.Count == 0)
            {
                return;
            }

            string[] propertyNames = ((MenuItem)sender).Tag.ToString().Split(',');

            StringBuilder sb = new StringBuilder();

            foreach (Account account in accountsDataGrid.SelectedItems)
            {
                for (int i = 0; i <= propertyNames.Length - 1; i++)
                {
                    sb.Append(GetPropertyValue(account, propertyNames[i]));
                    if (i < propertyNames.Length - 1)
                    {
                        sb.Append(":");
                    }
                }
                if (accountsDataGrid.SelectedItems.Count > 1)
                {
                    sb.Append(Environment.NewLine);
                }
            }

            Clipboard.SetDataObject(sb.ToString());
        }
    }
}