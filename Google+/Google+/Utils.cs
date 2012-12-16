using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Media;
using Microsoft.Phone.Controls;

namespace Google_Plus
{
    public class Utils
    {
        /// <summary>
        /// Parse HTML-style string to Paragraph
        /// </summary>
        /// <param name="rawString">Raw HTML-style string</param>
        /// <param name="maxLength">Max content length</param>
        /// <param name="maxLineCount">Max line limitation</param>
        /// <returns>The Paragraph parsed</returns>
        public static Paragraph ProcessRawContent(string rawString, int maxLength = -1, int maxLineCount = -1)
        {
            Paragraph result = new Paragraph();
            rawString = rawString.Replace("<br />", "\r").Replace("<br>", "\r");
            int index = 0, length = rawString.Length, nextIndex = -1, contentLength = 0, blockLength = 0;
            bool isBold = false, isItalic = false, lastMarkIsBold = false, hasMore = false;
            if (maxLineCount != -1)
            {
                int lastBreakerIndex = 0;
                for (int i = 0; i < maxLineCount; i++)
                {
                    if (lastBreakerIndex + 1 < length &&
                        (lastBreakerIndex = rawString.IndexOf('\r', lastBreakerIndex + 1)) == -1)
                        break;
                }
                if (lastBreakerIndex != -1)
                {
                    hasMore = true;
                    length = lastBreakerIndex;
                }
            }
            while (index < length && (nextIndex = rawString.IndexOf('<', index)) != -1 && nextIndex < length)
            {
                if (maxLength != -1 && contentLength >= maxLength)
                    break;
                else if (nextIndex != index)
                {
                    blockLength = nextIndex - index;
                    contentLength += blockLength;
                    if (maxLength != -1 && contentLength >= maxLength)
                    {
                        result.Inlines.Add(new Run()
                        {
                            Text = HttpUtility.HtmlDecode(rawString.Substring(index, blockLength - (contentLength - maxLength))),
                            FontStyle = isItalic ? FontStyles.Italic : FontStyles.Normal,
                            FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal
                        });
                        break;
                    }
                    else
                        result.Inlines.Add(new Run()
                        {
                            Text = HttpUtility.HtmlDecode(rawString.Substring(index, blockLength)),
                            FontStyle = isItalic ? FontStyles.Italic : FontStyles.Normal,
                            FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal
                        });
                    index = nextIndex;
                }
                switch (rawString[++index])
                {
                    case '/':
                        if (lastMarkIsBold)
                            isBold = false;
                        else
                            isItalic = false;
                        index += 3;
                        break;
                    case 'i':
                        isItalic = true;
                        lastMarkIsBold = false;
                        index += 2;
                        break;
                    case 'b':
                        isBold = true;
                        lastMarkIsBold = true;
                        index += 2;
                        break;
                    case 's':
                        if (rawString[++index] == '>')
                        {
                            //TODO: add strickthoughout
                            index++;
                        }
                        else
                        {
                            result.Inlines.Add(new Run()
                            {
                                Text = "+",
                                FontStyle = isItalic ? FontStyles.Italic : FontStyles.Normal,
                                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal
                            });

                            Hyperlink mention = new Hyperlink()
                            {
                                TextDecorations = null,
                                Foreground = new SolidColorBrush(Color.FromArgb(255, 162, 162, 255))
                            };

                            nextIndex = rawString.IndexOf("oid", index) + 5;
                            string uid = rawString.Substring(nextIndex, rawString.IndexOf('"', nextIndex) - nextIndex);

                            nextIndex = rawString.IndexOf('>', nextIndex) + 1;
                            blockLength = rawString.IndexOf('<', nextIndex) - nextIndex;
                            contentLength += blockLength + 1;

                            mention.Inlines.Add(new Run()
                            {
                                Text = HttpUtility.HtmlDecode(rawString.Substring(nextIndex, blockLength)),
                                FontStyle = isItalic ? FontStyles.Italic : FontStyles.Normal,
                                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal
                            });
                            mention.NavigateUri = new Uri("//Pages/ProfilePage.xaml?uid={0}".FormatWith(uid), UriKind.Relative);
                            result.Inlines.Add(mention);

                            index = rawString.IndexOf("n>", nextIndex) + 2;
                        }
                        break;
                    case 'a':
                        nextIndex = rawString.IndexOf("href=", index) + 5;
                        string mark = rawString.Substring(nextIndex, 1);
                        nextIndex++;
                        Uri uri = new Uri(rawString.Substring(nextIndex, rawString.IndexOf(mark, nextIndex) - nextIndex));

                        nextIndex = rawString.IndexOf('>', index) + 1;

                        Hyperlink link = new Hyperlink()
                        {
                            TextDecorations = null,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 162, 162, 255)),
                        };
                        string text = HttpUtility.HtmlDecode(rawString.Substring(nextIndex, rawString.IndexOf('<', nextIndex) - nextIndex));
                        contentLength += text.Length;
                        link.Inlines.Add(new Run()
                        {
                            Text = text,
                            FontStyle = isItalic ? FontStyles.Italic : FontStyles.Normal,
                            FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                        });
                        if (rawString[index + 2] == 'h')
                            link.NavigateUri = uri;
                        else
                            link.NavigateUri = new Uri("//Pages/Search.xaml?tag={0}".FormatWith(text), UriKind.Relative);
                        result.Inlines.Add(link);

                        index = rawString.IndexOf('>', nextIndex) + 1;
                        break;
                }
            }
            if (maxLength != -1)
            {
                if (contentLength > maxLength)
                    result.Inlines.Add(new Run() { Text = " More...", FontStyle = FontStyles.Italic });
                else if (length - index > maxLength - contentLength)
                {
                    result.Inlines.Add(new Run() { Text = HttpUtility.HtmlDecode(rawString.Substring(index, maxLength - contentLength)) });
                    result.Inlines.Add(new Run() { Text = " More...", FontStyle = FontStyles.Italic });
                }
                else
                {
                    result.Inlines.Add(new Run() { Text = HttpUtility.HtmlDecode(rawString.Substring(index, length - index)) });
                    if (hasMore)
                        result.Inlines.Add(new Run() { Text = " More...", FontStyle = FontStyles.Italic });
                }
            }
            else
            {
                result.Inlines.Add(new Run() { Text = HttpUtility.HtmlDecode(rawString.Substring(index, length - index)) });
                if (hasMore)
                    result.Inlines.Add(new Run() { Text = " More...", FontStyle = FontStyles.Italic });
            }
            return result;
        }

        private static DateTime startTime = new DateTime(1970, 1, 1);
        public static long Time { get { return (long)(DateTime.UtcNow - startTime).TotalMilliseconds; } }
        public static DateTime ToDateTime(double ticks)
        {
            return startTime.AddMilliseconds(ticks).ToLocalTime();
        }
    }

    public static class StringExtensions
    {
        /// <summary>
        /// Replaces the format item in a specified string with the string representation
        /// of a corresponding object in a specified array.
        /// </summary>
        /// <param name="format">A composite format string (see Remarks).</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>
        /// A copy of format in which the format items have been replaced by the string
        /// representation of the corresponding objects in args.
        /// </returns>
        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
    }

    public static class OrientationExtensions
    {
        /// <summary>
        /// Returns whether the page orientation is in portrait.
        /// </summary>
        /// <param name="orientation">Page orientation</param>
        /// <returns>If the orientation is portrait</returns>
        public static bool IsPortrait(this PageOrientation orientation)
        {
            return (PageOrientation.Portrait == (PageOrientation.Portrait & orientation));
        }
    }

    public class AppSettings
    {
        IsolatedStorageSettings settings;

        public AppSettings()
        {
            settings = IsolatedStorageSettings.ApplicationSettings;
        }

        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (settings.Contains(Key))
            {
                // If the value has changed
                if (settings[Key] != value)
                {
                    // Store the new value
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                settings.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (settings.Contains(Key))
                value = (T)settings[Key];
            // Otherwise, use the default value.
            else
                value = defaultValue;
            return value;
        }

        public void Save()
        {
            settings.Save();
        }
    }
}
