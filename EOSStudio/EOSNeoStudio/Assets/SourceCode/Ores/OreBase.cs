using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eos.Objects;
using Photon.Pun;

namespace Eos.Ores
{
    public partial class OreBase : ReferPlayer
    {
        protected long _oreid;
        protected uint _gearownerid;
        public bool Loaded;

        public uint GearOwnerID
        {
            get
            {
                return _gearownerid;
            }
            set { _gearownerid = value; }
        }
        public long OreId
        {
            set { _oreid = value;}
            get { return _oreid; }

        }

        public virtual void LoadOre()
        {
            throw new NotImplementedException();
        }
    }
}