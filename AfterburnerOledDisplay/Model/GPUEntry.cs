using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AfterburnerOledDisplay.Model
{
    public class GPUEntry : BaseModel
    {
        public GPUEntry(string name, string gpuid, int id)
        {
            _name = name;
            _gpuid = gpuid;
            _id = id;
        }

        private string _name;
        public string Name 
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _gpuid;
        public string GPUID 
        {
            get => _gpuid;
            set
            {
                if(_gpuid != value)
                {
                    _gpuid = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _id;
        public int ID 
        {
            get => _id;
            set
            {
                if(_id != value)
                {
                    _id = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                GPUEntry p = (GPUEntry)obj;
                return (Name == p.Name) && (GPUID == p.GPUID) && (ID == p.ID);
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() * GPUID.GetHashCode() * ID;
        }
    }
}
