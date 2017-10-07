using Android.Support.V7.Widget;
using Android.Views;
using System;
using System.Collections.Generic;
using VISE.Models;

namespace VISE.Adapters
{
    internal class ComputerAdapter : RecyclerView.Adapter
    {
        public ComputerAdapter(List<ComputerModel> computers)
        {
            //computers.AddRange(Globals.ComputerModels.ToList());
            //computers = computers.OrderBy(i => i.IsPinned).ToList();
            Computers = computers;
        }

        public List<ComputerModel> Computers { get; set; }

        public event EventHandler<int> ConnectClick;

        public event EventHandler<int> RemoveClick;

        public event EventHandler<int> PinClick;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (!(holder is ComputerViewHolder vh)) return;
            vh.Caption.Text = Computers[position].Ip;
            vh.ConnectButton.Click += (sender, args) => ConnectClick?.Invoke(holder, position);
            vh.RemoveButton.Click += (sender, args) => RemoveClick?.Invoke(holder, position);
            vh.PinButton.Click += (sender, args) => PinClick?.Invoke(holder, position);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.computercardview, parent, false);
            var vh = new ComputerViewHolder(itemView);
            return vh;
        }

        public override int ItemCount => Computers.Count;
    }
}