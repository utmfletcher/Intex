namespace Intex.Models
{

    public class MinMaxScaler
    {
        private double min;
        private double max;

        // Constructor to initialize the minimum and maximum values
        public MinMaxScaler(double min, double max)
        {
            this.min = min;
            this.max = max;
        }

        // Method to scale a value
        public double Scale(double value)
        {
            return (value - min) / (max - min);
        }

        // Method to rescale back to original value (if needed)
        public double Rescale(double scaledValue)
        {
            return scaledValue * (max - min) + min;
        }
    }
}
