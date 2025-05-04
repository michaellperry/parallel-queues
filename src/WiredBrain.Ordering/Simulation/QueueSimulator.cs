using MathNet.Numerics.Distributions;

namespace WiredBrain.Ordering.Simulation;

public class QueueSimulator
{
    private readonly Random _random = new();

    // Method to generate inter-arrival times with a given mean and CV
    public double GenerateRandomTime(double mean, double cv)
    {
        if (cv < double.Epsilon)
        {
            // If CV is 0, return the mean directly
            return mean;
        }

        // Calculate the shape (alpha) and rate (lambda) parameters for the Gamma distribution
        double shape = 1 / (cv * cv);  // Shape parameter: alpha = 1 / CV^2
        double rate = 1 / (mean * cv * cv);  // Rate parameter: lambda = 1 / (mean * CV^2)

        // Create a Gamma distribution for the service time using the calculated parameters
        var gammaDist = new Gamma(shape, rate, _random);

        // Sample from the Gamma distribution (this gives the service time)
        return gammaDist.Sample();
    }
}
