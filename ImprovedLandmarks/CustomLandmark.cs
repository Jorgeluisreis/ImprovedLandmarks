namespace ImprovedLandmarks
{
    public class CustomLandmark
    {
        public string Name { get; set; }
        public string PlanetName { get; set; }
        public float MapOffsetX { get; set; }
        public float MapOffsetY { get; set; }
        public float NormalAngle { get; set; } = 90f;
        public int PinSize { get; set; } = 15;
        public int LabelSize { get; set; } = 40;
        public int LabelSpacing { get; set; } = 4;
    }
}
