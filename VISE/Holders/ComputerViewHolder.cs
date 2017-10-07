using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace VISE
{
    public class ComputerViewHolder : RecyclerView.ViewHolder
    {
        public TextView Caption { get; private set; }
        public TextView Available { get; private set; }
        public FloatingActionButton PinButton { get; private set; }
        public Button RemoveButton { get; private set; }
        public Button ConnectButton { get; private set; }

        public ComputerViewHolder(View itemView) : base(itemView)
        {
            // Locate and cache view references:
            Caption = itemView.FindViewById<TextView>(Resource.Id.textView);
            Available = itemView.FindViewById<TextView>(Resource.Id.availableTextView);
            PinButton = itemView.FindViewById<FloatingActionButton>(Resource.Id.pinButton);
            RemoveButton = itemView.FindViewById<Button>(Resource.Id.removeButton);
            ConnectButton = itemView.FindViewById<Button>(Resource.Id.connectButton);
        }
    }
}