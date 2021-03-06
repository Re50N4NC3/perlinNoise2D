using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_controller : MonoBehaviour{
    public GameObject boidPrefab;  //object that is controlled for visuals
    List<GameObject> boidList = new List<GameObject>();  // list of the boids
    List<GameObject> boidNoiseList = new List<GameObject>();  // list of the noisy boids

    public readonly float gap = 0.0f;  // gap between the boids

    readonly static int outputSize = 64;  // size of the output array
    static int chunkSize = outputSize;  // size of the chunk

    public int octaveCount = 5;  // amount of calculated octaves
    public float offset = 16.0f;  // final scaling of the boids
    public float genHeight = 3.0f;  // height in calculations
    public float genBias = 1.0f;  // calculations bias (height multi)
    public int seed = 20;  // random seed

    public bool movement = true;  // checks if moevement is activated
    public int spd = 0;  // speed of the noise change
    float spdDelay = 0.0f;  // counter for move script
    readonly float spdDelayMax = 0.1f;  // time between moves

    readonly float[] noiseSeed = new float[outputSize * outputSize];  // create an array with the seed 1D
    readonly float[] perlinOut1D = new float[outputSize];  // create and array with a perlin noise output 1D
    readonly float[, ] noiseSeed2D = new float[outputSize, outputSize];  // create an array with the seed 2D
    readonly float[, ] perlinOut2D = new float[outputSize, outputSize];  // create and array with a perlin noise output 2D
    float[, ] chunkNoise = new float[chunkSize, chunkSize];
    float[,] chunkPerlin = new float[chunkSize, chunkSize];  // geretae arrays for the chunk
    public int chunkDepth = 0;  // checks how deep array went into the chunk

    // Start is called before the first frame update
    void Start() {
        // fill the noise array with random noise
        RandomNoise();  // generate random noise

        CreateBoids2D();  // create perlin boids

        ChunkGenerate();  // generate next chunk

        // generate 2d perlin noise
        int x;
        int y;

        for (x = 0; x < outputSize; x++) {
            for (y = 0; y < outputSize; y++) {
                perlinOut2D[x, y] = PerlinNoise2D(outputSize, noiseSeed2D, octaveCount, x, y, genHeight, genBias);  // perlin noise save
            }
        }
    }

    // Update is called once per frame
    void Update(){
        // noise movement
        spdDelay += Time.deltaTime;  // increase move coutner

        if (spdDelay >= spdDelayMax) {
            //RandomNoise();  // recalculate the seed
            //NoiseMove();  // move the noise

            // move the boids
            if (movement == true) {
                BoidMove();
            }

            // generate 2d perlin noise
            if (movement == false) {
                int x;
                int y;

                for (x = 0; x < outputSize; x++) {
                    for (y = 0; y < outputSize; y++) {
                        perlinOut2D[x, y] = PerlinNoise2D(outputSize, noiseSeed2D, octaveCount, x, y, genHeight, genBias);  // perlin noise save
                    }
                }
            }

            // update and timer reset
            ControlBoids2D();  // update boids
            spdDelay = 0;  // reset counter
        }

    }

    // generate perlin noise 1D
    float PerlinNoise1D(int size, float[] nSeed, int octaves, int iteration, float height, float bias) {
        int i;
        
        float noise = 0.0f;  // gather noise values
        float scale = 1.0f;  // scale variable for visuals
        float scaleAcc = 0.0f;  // combine all scale values

        // go through defined octaves
        for (i = 0; i < octaves; i++) {
            int pitch = size >> i;

            // calculate sample values
            int sample1 = (iteration / pitch) * pitch;  // div and multi to prevent rounding and leftover mistakes
            int sample2 = (sample1 + pitch) % size;  // modulo allows for the wraparound

            float blend = (float)(iteration - sample1) / (float)pitch;  // calculate for the values between 0 and 1
            float sample = (1.0f - blend) * nSeed[sample1] + blend * nSeed[sample2];  // linterp between the sample values
            noise = sample * scale;  // increase output by scale value
            scaleAcc += scale;  // increase scale accumulator
            scale = scale / bias;  // decrease the scale value
        }
        float pOut = (noise / scaleAcc) * height;

        return pOut;
    }

    // create boids and save them in the list 1D
    void CreateBoids1D() {
        int i;

        for (i = 0; i < outputSize; i++) {
            GameObject boid = Instantiate(boidPrefab) as GameObject;  // instatntiate the boid
            boid.transform.position = new Vector3((boid.transform.localScale.x + gap) * i, perlinOut1D[i]*100, 0);  // position the boid
            boidList.Add(boid as GameObject);  // add created boid to the list
        }
    }

    // control the boids 1D
    void MoveBoids1D() {
        int i;

        for (i = 0; i < outputSize; i++) {
            GameObject boid = boidList[i] as GameObject;

            boid.transform.position = new Vector3(0, perlinOut1D[i], 0);
        }
    }

    // create noisy boids 1D
    void NoiseBoids1D() {
        int i;

        for (i = 0; i < outputSize; i++) {
            GameObject boid = Instantiate(boidPrefab) as GameObject;  // instatntiate the boid
            boid.transform.position = new Vector3((boid.transform.localScale.x + gap) * i, noiseSeed[i], -1 - gap);  // position the boid
            boidNoiseList.Add(boid as GameObject);  // add created boid to the list
        }

    }

    // generate perlin noise 2D
    float PerlinNoise2D(int size, float[, ] nSeed, int octaves, int iterationX, int iterationY, float height, float bias) {
        int i;

        float noise = 0.0f;  // gather noise values
        float scale = 1.0f;  // scale variable for visuals
        float scaleAcc = 0.0f;  // combine all scale values

        // go through defined octaves
        for (i = 0; i < octaves; i++) {
            int pitch = size >> i;

            // calculate sample values
            int sampleX1 = (iterationX / pitch) * pitch;  // div and multi to prevent rounding and leftover mistakes X
            int sampleY1 = (iterationY / pitch) * pitch;  // div and multi to prevent rounding and leftover mistakes Y

            int sampleX2 = (sampleX1 + pitch) % size;  // modulo allows for the wraparound X
            int sampleY2 = (sampleY1 + pitch) % size;  // modulo allows for the wraparound Y

            float blendX = (float)(iterationX - sampleX1) / (float)pitch;  // calculate for the values between 0 and 1 X
            float blendY = (float)(iterationY - sampleY1) / (float)pitch;  // calculate for the values between 0 and 1 Y

            float sampleX = (1.0f - blendX) * nSeed[sampleY1, sampleX1] + blendX * nSeed[sampleY1, sampleX2];
            float sampleY = (1.0f - blendX) * nSeed[sampleY2, sampleX1] + blendX * nSeed[sampleY2, sampleX2];

            noise = (blendY * (sampleY - sampleX) + sampleX) * scale;  // linterp between two samples
            scaleAcc += scale;  // increase scale accumulator
            scale = scale / bias;  // halve the scale value
        }
        float pOut = (noise / scaleAcc) * height;

        return pOut;
    }

    // create boids and save them in the list 2D
    void CreateBoids2D() {
        int x;
        int y;

        for (x = 0; x < outputSize; x++) {
            for (y = 0; y < outputSize; y++) {
                GameObject boid = Instantiate(boidPrefab) as GameObject;  // instatntiate the boid
                boid.transform.localScale = new Vector3(1, perlinOut2D[x, y] * offset, 1);  // scale the boid
                boid.transform.position = new Vector3((boid.transform.localScale.x + gap) * x, Mathf.Abs((perlinOut2D[x, y] * offset) /2), (boid.transform.localScale.z + gap) * y);  // position the boid
                boidList.Add(boid as GameObject);  // add created boid to the list

                boid.GetComponent<Scr_boid>().xx = x;
                boid.GetComponent<Scr_boid>().yy = y;  // set grid position for created boid  (mainly debugging)
            }
        }
    }

    // create noisy boids 2D
    void NoiseBoids2D() {
        int x;
        int y;

        for (x = 0; x < outputSize; x++) {
            for (y = 0; y < outputSize; y++) {
                GameObject boid = Instantiate(boidPrefab) as GameObject;  // instatntiate the boid
                boid.transform.localScale = new Vector3(1, 1, 1);  // scale the boid
                boid.transform.position = new Vector3(((boid.transform.localScale.x + gap) * x) + (gap+1)*20+ (outputSize * (boid.transform.localScale.x + gap)), Mathf.Abs((noiseSeed[y + x * outputSize] * offset) / 2), (boid.transform.localScale.z + gap) * y);  // position the boid
                boidNoiseList.Add(boid as GameObject);  // add created boid to the list
            }
        }
    }

    // control and update the boids real time 2D
    void ControlBoids2D() {
        int x;
        int y;

        for (x = 0; x < outputSize; x++) {
            for (y = 0; y < outputSize; y++) {
                GameObject boid = boidList[y + x * outputSize] as GameObject;
                boid.GetComponent<Scr_boid>().targetScale = perlinOut2D[x, y] * offset;  // scale the boid

                boid.GetComponent<Scr_boid>().targetX = (boid.transform.localScale.x + gap) * x;  // position the boid
                boid.GetComponent<Scr_boid>().targetY = (perlinOut2D[x, y] * offset) / 2;  // position the boid
                boid.GetComponent<Scr_boid>().targetZ = (boid.transform.localScale.z + gap) * y;  // position the boid
            }
       }
    }

    // calculate random noise based on seed change
    void RandomNoise() {
        int x;
        int y;

        Random.InitState(seed);  // change the seed
        for (x = 0; x < outputSize * outputSize; x++) {
            noiseSeed[x] = Random.Range(0.0f, 1.0f);  // recalculate random values
        }

        for (x = 0; x < outputSize; x++) {
            for (y = 0; y < outputSize; y++) {
                noiseSeed2D[x, y] = Random.Range(0.0f, 1.0f);  // recalculate random values
            }
        }
    }

    // calculate random noise based on seed change with movement
    void NoiseMove() {
        int x;
        
        for (x = 0; x <= (outputSize * outputSize) - outputSize; x++) {
            noiseSeed[x] = noiseSeed[x + spd];//Random.Range(0.0f, 1.0f);  // recalculate random values
        }
        for (x = (outputSize * outputSize) - outputSize; x < (outputSize * outputSize); x++) {
            noiseSeed[x] = Random.Range(0.0f, 1.0f);  // recalculate random values
        }
    }

    // boids movement 2D
    void BoidMove() {
        int x;
        int y;
        
        for (x = outputSize - spd; x < outputSize; x++) {
            for (y = 0; y < outputSize; y++) {
                perlinOut2D[x, y] = chunkPerlin[chunkDepth, y];  // perlin noise create
            }
            chunkDepth += spd;  // update how deep you are into the chunk
        }

        if (chunkDepth >= outputSize - spd) {
            chunkDepth = 0;
            ChunkGenerate();
        }

        for (x = 0; x < outputSize - spd; x++) {
            for (y = 0; y < outputSize; y++) {
                perlinOut2D[x, y] = perlinOut2D[x + spd, y];  // move all values according to the speed
            }
        }
    }

    // chunk value assign TODO its now only for x
    void ChunkGenerate() {
        int x;
        int y;
        int s;

        for (s = 0; s < 2; s++) { 
            for (x = 0; x < chunkSize; x++) {
                for (y = 0; y < chunkSize; y++) {
                    // generate or read the noise
                    if (s == 0) {
                        if (x < chunkSize / 2) {
                            chunkNoise[x, y] = noiseSeed2D[x + (chunkSize / 2), y];  // read noise for the chunk
                        }
                        else {
                            chunkNoise[x, y] = Random.Range(0.0f, 1.0f);  // generate noise for the chunk
                        }
                    }

                    // calculate perlin
                    else {
                        chunkPerlin[x, y] = PerlinNoise2D(chunkSize, chunkNoise, octaveCount, x, y, genHeight, genBias);
                    }
                }
            }
        }
    }
}
