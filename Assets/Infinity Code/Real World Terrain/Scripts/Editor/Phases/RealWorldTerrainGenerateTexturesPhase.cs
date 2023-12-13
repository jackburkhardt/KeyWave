/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using InfinityCode.RealWorldTerrain.Generators;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainGenerateTexturesPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Generate Textures..."; }
        }

        public override void Enter()
        {
            if (index >= terrainCount)
            {
                Complete();
                return;
            }

            int x = index % prefs.terrainCount.x;
            int y = index / prefs.terrainCount.x;

            progress = (index + phaseProgress) / terrainCount;

            if (prefs.textureResultType == RealWorldTerrainTextureResultType.regularTexture) RealWorldTerrainTextureGenerator.GenerateTexture(terrains[x, y]);
            else if (prefs.textureResultType == RealWorldTerrainTextureResultType.hugeTexture) RealWorldTerrainTextureGenerator.GenerateHugeTexture(terrains[x, y]);
            else if (prefs.textureResultType == RealWorldTerrainTextureResultType.terrainLayers) RealWorldTerrainTerrainLayersGenerator.Generate(terrains[x, y]);
            else phaseComplete = true;

            if (phaseComplete)
            {
                index++;
                phaseProgress = 0;
                phaseComplete = false;
            }
        }

        public override void Finish()
        {
            RealWorldTerrainTextureGenerator.colors = null;
        }
    }
}