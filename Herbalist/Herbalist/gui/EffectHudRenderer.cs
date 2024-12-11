using System.Collections.Generic;
using Herbalist.effect;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Herbalist.gui;

public class EffectHudRenderer : IRenderer
{
    private ICoreClientAPI _api;

    private Dictionary<string, LoadedTexture> _textures = new();
    private LoadedTexture effectBackground;
    private EffectManager _effectManager;

    public EffectHudRenderer(ICoreClientAPI api, EffectRegistry registry,EffectManager manager)
    {
        _api = api;
        _effectManager = manager;
        effectBackground = new(_api);
        api.Render.GetOrLoadTexture(new AssetLocation("herbalist:textures/gui/effecticons/background.png"),
            ref effectBackground);

        foreach (KeyValuePair<string, IPlayerEffect> registryItem in registry.EffectReg)
        {
            LoadedTexture texture = new(_api);
            api.Render.GetOrLoadTexture(new AssetLocation(registryItem.Value.EffectIconLocation()), ref texture);
            _textures[registryItem.Key] = texture;
        }
    }


    public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
    {

        int startX = 25;
        int offset = 8;

        int rendered = 0;
        int distanceApart = 100;
        
        foreach (var currentEffect in _effectManager.ClientSideEffectsWithDuration)
        {
            
            _textures.TryGetValue(currentEffect.Key, out LoadedTexture texture);

            if (texture == null)
            {
                _api.Logger.Warning($"Effect {currentEffect.Key} has no valid texture!");
                continue;
            }
            
            _api.Render.Render2DTexturePremultipliedAlpha(effectBackground.TextureId, startX + (rendered * distanceApart), 25, 80, 80);
            _api.Render.Render2DTexturePremultipliedAlpha(
                texture.TextureId,
                startX + (rendered * distanceApart) + offset, 33,
                64, 64
            );
            rendered++;
        }
    }

    public double RenderOrder => 0.5;
    public int RenderRange => 1000;

    public void Dispose()
    {
        foreach (LoadedTexture loadedTexture in _textures.Values)
        {
            loadedTexture.Dispose();
        }
    }
}