using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/**
* @Author: Churong Zhang
* @Email: churong.zhang@utdallas.edu
* @Date:  4/10/2020
* This is the comparer for the list view
*/
namespace Multithreaded_Text_Search
{
    class ListViewItemCompare : IComparer
    {
        private SortOrder order;
        public ListViewItemCompare()
        {   // default sorting to be ascending
            order = SortOrder.Ascending;
        }
        public ListViewItemCompare(bool decending)
        {   // set the sorting order 
            if (decending)
                order = SortOrder.Descending;
            else
                order = SortOrder.Ascending;
        }

        public int Compare(object x, object y)
        {
            int ret = -1;
            ListViewItem x1 = (ListViewItem)x;
            ListViewItem x2 = (ListViewItem)y;
            // get the int value and compare them
            int v1 = int.Parse(x1.SubItems[0].Text);
            int v2 = int.Parse(x2.SubItems[0].Text);
            ret = v1 - v2;
            // negate it if is descending
            if (order == SortOrder.Descending)
                ret = -ret;
            return ret;
        }
    }
}
