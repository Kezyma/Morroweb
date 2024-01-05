namespace EspConverter.Models.Web
{
    public class WebWeapon : WebItem
    {
        public string Enchanting { get; set; }
        public string WeaponType {  get; set; }
        public int Health {  get; set; }
        public float Speed { get; set; }
        public float Reach {  get; set; }
        public int Enchantment { get; set; }
        public int[] Chop { get; set; }
        public int[] Slash { get; set; }
        public int[] Thrust { get; set; }
    }
}
