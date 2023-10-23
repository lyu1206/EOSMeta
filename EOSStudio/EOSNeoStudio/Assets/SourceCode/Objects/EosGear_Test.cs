using Eos.Objects;

namespace Eos.Objects
{
    public partial class EosGear
    {
        //2023-3-18 주석 단체 테스트 코드 잘지워라
        //2023-3-25 주석 단체 테스트 gear_tear코드랑 새롭게 ByteLoader 코드 잘지켜서 유지해라!! 그리고 주석해야하는디..으..
        public  void test_GearOnActivate(bool active)
        {
            if (!(_parent is EosPawnActor pawn))
                return;
            OnReady();
        }
    }
}