using System.Text;
namespace Droneboi1Keyloader
{
	public class Position
	{
		public float x = 0;
		public float y = 0;
		public float r = 0;
		public Position(float x, float y, float r)
		{
			this.x = x;
			this.y = y;
			this.r = r;
		}
		public string ToKey()
		{
			return $"{this.x}~{this.y};{this.r}";
		}
		public Position(string str)
		{
			string[] a = str.Split(';');
			string[] p = a[0].Split('~');
			this.x = float.Parse(p[0]);
			this.y = float.Parse(p[1]);
			this.r = float.Parse(a[1]);
		}
		public Position(string[] array)
		{
			string[] p = array[0].Split('~');
			this.x = float.Parse(p[0]);
			this.y = float.Parse(p[1]);
			this.r = float.Parse(array[1]);
		}
		public static Position preset = new Position(0, 0, 0);
	}
	public enum FiringGroup
    {

		PrimaryOrNone = 0,
		Secondary,
    }
	public enum ControlGroup
    {
		One = 0,
		Two,
		Three,
		Four,
		Five,
    }
	public enum Paint
    {
		White = 0,
		Black,
		Blue,
		Orange,
		Red,
		Green,
		Yellow,
		RedGift,
		Grey,
		PinkSwirl,
		CautionStripes,
		GreenishYellow,
		FadedRed,
		BrownWood,
		WhiteCrate, 
		Purple,
		Pink,
		GreenGift,
		Snowman,
    }
	public enum Direction
    {
		AutoOrNone = 0,
		Up,
		Down,
		Left,
		Right,
    }
	public class Block
	{
		private enum SpecialType
		{
			Normal,
			Propulsion,
		}
		private string _name = string.Empty;
		public string name
        {
            get => _name;
            set
            {
                if (value.Contains("Thruster") || value.Contains("Wheel") || value.Contains("burner",StringComparison.OrdinalIgnoreCase))
                {
                    this.type = SpecialType.Propulsion;
                    if (this.force == null) this.force = 0;
                }
                else
                {
                    this.type = SpecialType.Normal;
                    if (this.firingGroup == null) this.firingGroup = FiringGroup.PrimaryOrNone;
                }
                _name = value;
            }
        }
        public Position position = Position.preset;
		public FiringGroup? firingGroup = null;
		public float? force = null;
		public List<ControlGroup> controlGroups = new();
		public Paint paint = Paint.White;
		public Direction thrusterInputDirection = Direction.AutoOrNone;
		public bool flip = false;
		private SpecialType type = SpecialType.Normal;
		public Block() {}
		public Block(string str)
		{
			string[] v = str.Split(';');

			this.name = v[0];
			this.position = new Position(new string[] { v[1] , v[2] });
			if (this.name.Contains("Thruster") || this.name.Contains("Wheel") || this.name.Contains("burner", StringComparison.OrdinalIgnoreCase))
			{
				this.type = SpecialType.Propulsion;
				this.force = float.Parse(v[3]);
            }
			else
            {
                this.firingGroup = v[3] == "0" ? FiringGroup.PrimaryOrNone : FiringGroup.Secondary;
            }

			// Control groups
			string cgStr = v[4];
			if (cgStr != "-1")
			{
				if (cgStr.Contains("0")) this.controlGroups.Add(ControlGroup.One);
				if (cgStr.Contains("1")) this.controlGroups.Add(ControlGroup.Two);
				if (cgStr.Contains("2")) this.controlGroups.Add(ControlGroup.Three);
				if (cgStr.Contains("3")) this.controlGroups.Add(ControlGroup.Four);
				if (cgStr.Contains("4")) this.controlGroups.Add(ControlGroup.Five);
			}

			this.paint = (Paint)int.Parse(v[5]);
			this.thrusterInputDirection = (Direction)int.Parse(v[6]);
			this.flip = v[7] == "1";
		}
		public string ToKey()
		{
			// Control groups
			string cgStr = "";
			if (this.controlGroups.Count != 0)
			{
				foreach (ControlGroup cg in this.controlGroups)
				{
					cgStr += ((int)cg).ToString();
				}
			}
			else cgStr = "-1";

			return $"{this.name};{this.position.ToKey()};{((this.type == SpecialType.Normal) ? ((int)this.firingGroup) : this.force)};{cgStr};{((int)this.paint)};{((int)this.thrusterInputDirection)};{(this.flip ? "1" : "0")}";
		}
	}

	public class Vehicle
	{
		public Position position;
		public List<Block> blocks = new();
        public int GetBlockCount()
        { return this.blocks.Count; }
        public Vehicle(string key)
		{
			string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(key));

			string[] sections = decoded.Split('|');

			// Spawn offset
			{
				string section = sections[0];
				string[] parts = section.Split(';');
				this.position = new Position(new string[] { parts[1] , parts[2] });
			}

			// Blocks
			{
				string section = sections[2];
				string[] array = section.Split(':');
				string[] blockStrArray = array.SkipLast(1).ToArray();
				foreach (string blockStr in blockStrArray)
				{
					this.blocks.Add(new Block(blockStr));
				}
			}
		}
		public string ToKey()
		{
			string blocksStr = "";
			foreach (Block block in this.blocks)
			{
				blocksStr += block.ToKey() + ':';
			}
			return Convert.ToBase64String(Encoding.UTF8.GetBytes($"PlayerVehicle;{this.position.ToKey()}|{this.GetBlockCount()}|{blocksStr}"));
		}
	}
}