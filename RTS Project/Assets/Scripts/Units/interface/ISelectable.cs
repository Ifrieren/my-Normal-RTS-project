
using RTS.EventSystem;
namespace RTS.Units
{
    public interface ISelectable
    {
        void OnSelected(UnitSelectEvent evt);
        void OndeSelected(UnitDeSelectEvent evt);
    }


}