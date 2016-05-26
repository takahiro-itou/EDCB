﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace EpgTimer
{
    public class ListViewSelectedKeeper
    {
        //リスト番組表で全選択状態でチャンネル選択更新してしまったりしたときなどでも大丈夫なように、
        //一応選択数の上限を設定しておく。
        public uint MaxRestoreNum = 100;

        public ListBox listBox = null;
        public ulong? oldItem = null;
        public List<ulong> oldItems = null;
        public bool allSelected = false;
        protected Func<object, ulong> getKey = null;

        public ListViewSelectedKeeper(ListBox list, bool DoStoringNow = false, Func<object, ulong> _key = null)
        {
            listBox = list;
            getKey = _key;
            if (DoStoringNow) StoreListViewSelected();
        }

        public void StoreListViewSelected()
        {
            if (listBox != null && listBox.SelectedItem != null)
            {
                getKey = getKey ?? CtrlCmdDefEx.GetKeyFunc(listBox.SelectedItem.GetType());
                oldItem = getKey(listBox.SelectedItem);
                oldItems = listBox.SelectedItems.Cast<object>().Select(data => getKey(data)).ToList();
                allSelected = (oldItems.Count != 1 && oldItems.Count == listBox.Items.Count);
            }
        }

        public void RestoreListViewSelected(ListBox list = null)
        {
            try
            {
                if (list != null) listBox = list;
                if (listBox != null && listBox.Items.Count != 0 && oldItem != null && oldItems != null)
                {
                    if (this.allSelected == true)
                    {
                        listBox.SelectAll();
                        return;
                    }

                    //このUnselectAll()は無いと正しく復元出来ない状況があり得る
                    listBox.UnselectAll();

                    //上限越えの場合は、選択を解除して終了。
                    if (oldItems.Count >= this.MaxRestoreNum) return;

                    //選択数が少ないときは逆に遅くなる気もするが、Dictionaryにしておく
                    var listKeys = new Dictionary<ulong, object>();
                    getKey = getKey ?? CtrlCmdDefEx.GetKeyFunc(listBox.Items[0].GetType());

                    foreach (object listItem1 in listBox.Items)
                    {
                        //重複するキーは基本的に無いという前提
                        try
                        {
                            listKeys.Add(getKey(listItem1), listItem1);
                        }
                        catch { }
                    }

                    object setItem;
                    if (listKeys.TryGetValue((ulong)oldItem, out setItem))
                    {
                        listBox.SelectedItem = setItem;
                    }

                    foreach (ulong oldItem1 in oldItems)
                    {
                        if (listKeys.TryGetValue(oldItem1, out setItem))
                        {
                            //数が多いとき、このAddが致命的に遅い
                            listBox.SelectedItems.Add(setItem);
                        }
                    }

                    //画面更新が入るので最後に実行する。SelectedItem==nullのときScrollIntoViewは何もしない。
                    listBox.ScrollIntoView(listBox.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

    }

}
