/*         INFINITY CODE         */
/*   https://infinity-code.com   */

namespace InfinityCode.RealWorldTerrain
{
    public enum RealWorldTerrainBuildR2Collider
    {
        none,
        primitive,
        simple,
        complex
    }

    public enum RealWorldTerrainBuildR2RenderMode
    {
        none,
        box,
        simple,
        full
    }

    public enum RealWorldTerrainBuildingBottomMode
    {
        followRealWorldData,
        followTerrain
    }

    public enum RealWorldTerrainByteOrder
    {
        Windows,
        Mac
    }

    public enum RealWorldTerrainElevationProvider
    {
        SRTM,
        BingMaps,
        SRTM30,
        Mapbox,
        //ArcGIS
    }

    public enum RealWorldTerrainElevationRange
    {
        autoDetect,
        fixedValue,
        realWorldValue
    }

    public enum RealWorldTerrainElevationType
    {
        realWorld,
        heightmap
    }

    public enum RealWorldTerrainGenerateBuildingPhaseEnum
    {
        house,
        wall
    }

    public enum RealWorldTerrainGenerateType
    {
        full,
        terrain,
        texture,
        additional
    }

    public enum RealWorldTerrainMapboxLayer
    {
        building,
        landuse_overlay,
        landuse,
        road,
        water,
        waterway,
        structure
    }

    public enum RealWorldTerrainMapboxLanduse
    {
        aboriginal_lands = 1,
        agriculture = 1 << 1,
        airport = 1 << 2,
        cemetery = 1 << 3,
        glacier = 1 << 4,
        grass = 1 << 5,
        hospital = 1 << 6,
        park = 1 << 7,
        piste = 1 << 8,
        pitch = 1 << 9,
        rock = 1 << 10,
        sand = 1 << 11,
        school = 1 << 12,
        scrub = 1 << 13,
        wood = 1 << 14,
        facility = 1 << 15
    }

    public enum RealWorldTerrainMapboxLanduseOverlay
    {
        national_park = 1,
        wetland = 1 << 1,
        wetland_noveg = 1 << 2
    }

    public enum RealWorldTerrainMapboxStructure
    {
        cliff = 1,
        fence = 1 << 1,
        gate = 1 << 2,
        hedge = 1 << 3,
        land = 1 << 4,
        steps = 1 << 5,
        tower = 1 << 6
    }

    public enum RealWorldTerrainMapboxWaterway
    {
        river = 1,
        canal = 1 << 1,
        stream = 1 << 2,
        stream_intermittent = 1 << 3,
        drain = 1 << 4,
        ditch = 1 << 5
    }

    public enum RealWorldTerrainMaxElevation
    {
        autoDetect,
        realWorldValue
    }

    public enum RealWorldTerrainOSMOverpassServer
    {
        main = 0,
        main2 = 1,
        french = 2,
        taiwan = 3,
        kumiSystems = 4,
    }

    public enum RealWorldTerrainRawType
    {
        RAW,
        mapboxRGB
    }

    public enum RealWorldTerrainResultType
    {
        terrain,
        mesh,
        gaiaStamp,
        rawFile
    }

    public enum RealWorldTerrainRoadType
    {
        motorway = 1,
        trunk = 1 << 1,
        primary = 1 << 2,
        secondary = 1 << 3,
        tertiary = 1 << 4,
        unclassified = 1 << 5,
        residential = 1 << 6,
        service = 1 << 7,
        motorway_link = 1 << 8,
        trunk_link = 1 << 9,
        primary_link = 1 << 10,
        secondary_link = 1 << 11,
        tertiary_link = 1 << 12,
        living_street = 1 << 13,
        pedestrian = 1 << 14,
        track = 1 << 15,
        bus_guideway = 1 << 16,
        raceway = 1 << 17,
        road = 1 << 18,
        footway = 1 << 19,
        cycleway = 1 << 20,
        bridleway = 1 << 21,
        steps = 1 << 22,
        path = 1 << 23
    }

    public enum RealWorldTerrainRoadTypeMode
    {
        simple,
        advanced
    }

    /// <summary>
    /// Type of building roof.
    /// </summary>
    public enum RealWorldTerrainRoofType
    {
        /// <summary>
        /// Dome roof.
        /// </summary>
        dome,

        /// <summary>
        /// Flat roof.
        /// </summary>
        flat
    }

    public enum RealWorldTerrainTextureFileType
    {
        png,
        jpg
    }

    public enum RealWorldTerrainTextureProvider
    {
        arcGIS,
        google,
        mapQuest,
        nokia,
        virtualEarth,
        openStreetMap,
        custom = 999
    }

    public enum RealWorldTerrainTextureResultType
    {
        regularTexture = 0,
        hugeTexture = 1,
        terrainLayers = 2
    }

    public enum RealWorldTerrainTextureType
    {
        satellite,
        terrain,
        relief
    }

    public enum RealWorldTerrainUpdateType
    {
        all,
        alpha,
        beta,
        releaseCandidate,
        stable
    }

    public enum RealWorldTerrainVolumeGrassOutsidePoints
    {
        removeOutsidePoints,
        noMakeAllMeshes,
        noMakeMeshesWithOutsidePoints
    }
}