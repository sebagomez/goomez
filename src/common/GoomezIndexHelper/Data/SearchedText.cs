using System;
using System.Data;
using System.Configuration;

namespace GoomezIndexHelper.Data
{
    /// <summary>
    /// Summary description for SearchedText
    /// </summary>
    public class TextSearched : IComparable<TextSearched>
    {
        public TextSearched() { }

        string m_user;
        string m_date;
        string m_text;

        public string VisitUser
        {
            get { return m_user; }
            set { m_user = value; }
        }

        public string DateTicks
        {
            get { return m_date; }
            set { m_date = value; }
        }

        public string SearchedText
        {
            get { return m_text; }
            set { m_text = value; }
        }

        public int CompareTo(TextSearched other)
        {
            DateTime mine = DateTime.ParseExact(DateTicks, "yyyyMMddHHmmss", System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat);
            DateTime others = DateTime.ParseExact(other.DateTicks, "yyyyMMddHHmmss", System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat);

            return others.CompareTo(mine);
        }

    }
}