using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureAuth.ReferenceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class DiamondSquareController : ControllerBase
{
    [HttpGet, Route("{magnitude}"), Authorize(Roles = "AlgorithmEvaluator")]
    public FileContentResult Get(int magnitude, float roughness = .8f)
    {
        var datafloats = RunDiamondSquare(magnitude, roughness);
        var databytes = new byte[datafloats.Length * sizeof(float)];
        Buffer.BlockCopy(datafloats, 0, databytes, 0, databytes.Length);
        return File(databytes, "application/octet-stream");
    }

    //https://en.wikipedia.org/wiki/Diamond-square_algorithm
    // (Follows the spirit of the Wikipedia article with quotes
    // from the article in comments.

    // The algorithm is implemented as a static method that takes
    // the map magnitude and roughness factor as parameters and
    // returns a one-dimensional array of floats representing the
    // map texture.

    // The map magnitude is the exponent of the width and height
    // of the map, so a magnitude of 8 would produce a map of
    // (2^8 + 1)^2 = 257 * 257 = 66049 pixels.

    // The roughness factor, h, is a value between 0.0 and 1.0 that
    // controls the roughness. Lower values produce rougher maps.)
    static float[] RunDiamondSquare(int mapMagnitude, float h)
    {
        // The diamond-square algorithm begins with a two-dimensional
        // square array of width and height 2^n + 1.
        var size = (int)Math.Pow(2, mapMagnitude) + 1;
        var data = new Span<float>(new float[size * size]);

        // (Data is stored in a one-dimensional array and idx is a helper
        // function to convert x and y coordinates to an index.)
        int idx(int x, int y) => x + y * size;

        // Each random value is multiplied by a scale constant, which
        // decreases with each iteration by a factor of 2^−h, where h is
        // a value between 0.0 and 1.0 (lower values produce rougher
        // terrain).
        var factor = 1.0f;
        float rnd() => (Random.Shared.NextSingle() * 2 - 1) * factor;
        void decrease() => factor *= MathF.Pow(2, -h);

        // The four corner points of the array must first be set to
        // initial values. The diamond and square steps are then
        // performed alternately until all array values have been set.
        var currentDistance = size - 1;
        data[0] = rnd(); data[currentDistance * size] = rnd();
        data[currentDistance] = rnd(); data[size * size - 1] = rnd();

        do
        {
            decrease();
            // (half distance is used several times to find the midpoint, so calculate it once)
            var halfDistance = currentDistance / 2;

            // The diamond step:
            // For each square in the array, set the midpoint of that
            // square to be the average of the four corner points plus
            // a random value.
            for (var offsetY = halfDistance; offsetY + 1 < size; offsetY += currentDistance)
            {
                for (var offsetX = halfDistance; offsetX + 1 < size; offsetX += currentDistance)
                {
                    float sum =
                        data[idx(offsetX - halfDistance, offsetY - halfDistance)] +
                        data[idx(offsetX - halfDistance, offsetY + halfDistance)] +
                        data[idx(offsetX + halfDistance, offsetY - halfDistance)] +
                        data[idx(offsetX + halfDistance, offsetY + halfDistance)];
                    data[idx(offsetX, offsetY)] = sum / 4 + rnd();
                }
            }

            // The square step: For each diamond in the array, set the
            // midpoint of that diamond to be the average of the four
            // corner points plus a random value.
            for (var offset1 = 0; offset1 < size; offset1 += currentDistance)
            {
                for (var offset2 = halfDistance; offset2 < size; offset2 += currentDistance)
                {
                    // During the square steps, points located on the
                    // edges of the array will have only three adjacent
                    // values set, rather than four. There are a number
                    // of ways to handle this complication - the simplest
                    // being to take the average of just the three
                    // adjacent values

                    // (The square step is jagged so the offsets are re-used
                    // as both x and y coordinates.)
                    float sum1 = 0, sum2 = 0; int count1 = 2, count2 = 2;
                    // (The value of offset2 is always within bounds, so those are
                    // safe to add, and the counts can be initialized to 2.)
                    sum1 += data[idx(offset1, offset2 - halfDistance)];
                    sum1 += data[idx(offset1, offset2 + halfDistance)];
                    sum2 += data[idx(offset2 - halfDistance, offset1)];
                    sum2 += data[idx(offset2 + halfDistance, offset1)];

                    // (But offset1 needs to be checked against the bounds.)
                    if (offset1 > 0)
                    {
                        sum1 += data[idx(offset1 - halfDistance, offset2)]; count1++;
                        sum2 += data[idx(offset2, offset1 - halfDistance)]; count2++;
                    }
                    if (offset1 < size - 1)
                    {
                        sum1 += data[idx(offset1 + halfDistance, offset2)]; count1++;
                        sum2 += data[idx(offset2, offset1 + halfDistance)]; count2++;
                    }
                    data[idx(offset1, offset2)] = sum1 * (1.0f / count1) + rnd();
                    data[idx(offset2, offset1)] = sum2 * (1.0f / count2) + rnd();
                }
            }
        } while ((currentDistance /= 2) > 1);
        return data.ToArray();
    }
}
