using Foundation;
using AppKit;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace OpenProfilerUI
{
	public class ComboBoxDataSource<T> : NSComboBoxDataSource
	{
		private NSComboBox _comboBox;

		public ObservableCollection<T> DataSource { get; } = new ObservableCollection<T>();

		public ComboBoxDataSource()
		{
			DataSource.CollectionChanged += DataSourceChanged;
		}

		private void DataSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_comboBox?.DeselectItem(_comboBox.SelectedIndex);
			_comboBox?.ReloadData();
		}

		protected override void Dispose(bool disposing)
		{
			DataSource.CollectionChanged -= DataSourceChanged;
			_comboBox?.Dispose();
			_comboBox = null;
			base.Dispose(disposing);
		}

		[Export("numberOfItemsInComboBox:")]
		public override nint ItemCount(NSComboBox comboBox)
		{
			if (_comboBox == null)
			{
				_comboBox = comboBox;
			}

			return DataSource.Count();
		}

		public Func<int, string> GetValue { get; set; }
		public Func<string, int> GetIndexOf { get; set; }

		[Export("comboBox:objectValueForItemAtIndex:")]
		public override NSObject ObjectValueForItem(NSComboBox comboBox, nint index)
		{
			if (_comboBox == null)
			{
				_comboBox = comboBox;
			}

			return new NSString(GetValue((int)index));
		}

		public override nint IndexOfItem(NSComboBox comboBox, string value)
		{
			if (_comboBox == null)
			{
				_comboBox = comboBox;
			}

			return GetIndexOf(value);
		}
	}
}
