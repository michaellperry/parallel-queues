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

        // Calculate the shape (k) and scale (theta) parameters for the Gamma distribution
        // In this case, μ is mean, not service rate
        double variance = cv * mean * cv * mean;  // σ = CV * μ, so variance = σ^2 = (CV * μ)^2
        double scale = variance / mean;  // scale = variance / mean
        double shape = mean * mean / variance;  // k = μ^2 / σ^2

        // Create a Gamma distribution for the service time
        var gammaDist = new Gamma(shape, scale, _random);

        // Sample from the Gamma distribution (this gives the service time)
        return gammaDist.Sample();

    }
}
