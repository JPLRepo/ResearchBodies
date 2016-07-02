
namespace ResearchBodies
{
    public class BodyIgnoreData
    {
        public bool Easy, Normal, Medium, Hard;
        public BodyIgnoreData(bool easy, bool normal, bool medium, bool hard)
        {
            Easy = easy;
            Normal = normal;
            Medium = medium;
            Hard = hard;
        }

        public void setBodyIgnoreData(bool easy, bool normal, bool medium, bool hard)
        {
            Easy = easy;
            Normal = normal;
            Medium = medium;
            Hard = hard;
        }

        public bool GetLevel(Level lvl)
        {
            bool x;
            switch (lvl)
            {
                case Level.Easy:
                    x = this.Easy;
                    break;
                case Level.Normal:
                    x = this.Normal;
                    break;
                case Level.Medium:
                    x = this.Medium;
                    break;
                default:
                    x = this.Hard;
                    break;
            }
            return x;
        }
        public override string ToString()
        {
            return this.Easy + " " + this.Normal + " " + this.Medium + " " + this.Hard;
        }
    }
}
