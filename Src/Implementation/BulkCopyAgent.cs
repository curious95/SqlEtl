using System;

namespace SqlEtl.Implementation
{
    [Serializable]
    public class BulkCopyAgent
    {
        public delegate void BulkInsertAppliedEvent(object sender, BulkInsertAppliedEventArgs e);

        public delegate void BulkInsertSelectedEvent(object sender, BulkInsertSelectedEventArgs e);

        public BulkInsertProvider LocalProvider { get; set; }
        public BulkInsertProvider RemoteProvider { get; set; }
        public event BulkInsertSelectedEvent ChangesSelected;
        public event BulkInsertAppliedEvent ChangesApplied;

        protected virtual void RaiseChangesSelected(BulkInsertSelectedEventArgs e)
        {
            ChangesSelected?.Invoke(this, e);
        }

        protected virtual void RaiseChangesApplied(BulkInsertAppliedEventArgs e)
        {
            ChangesApplied?.Invoke(this, e);
        }

        public int WriteToServer()
        {
            var d = LocalProvider.GetBulkRows();
            RaiseChangesSelected(new BulkInsertSelectedEventArgs(d));
            var ret = RemoteProvider.InsertBulkRows(d);
            RaiseChangesApplied(new BulkInsertAppliedEventArgs(d));

            return ret;
        }
    }
}