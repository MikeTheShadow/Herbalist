using System.Collections.Generic;
using Herbalist.effect;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Herbalist.gui;

public class EffectHudRenderer : IRenderer
{
    private readonly ICoreClientAPI _api;

    private readonly Dictionary<string, LoadedTexture> _textures = new();
    private readonly Dictionary<string, string> _langData = new();
    private readonly LoadedTexture _effectBackground;
    private readonly EffectManager _effectManager;

    public EffectHudRenderer(ICoreClientAPI api, EffectRegistry registry, EffectManager manager)
    {
        _api = api;
        _effectManager = manager;
        _effectBackground = new(_api);
        api.Render.GetOrLoadTexture(new AssetLocation("herbalist:textures/gui/effecticons/background.png"),
            ref _effectBackground);

        foreach (KeyValuePair<string, IPlayerEffect> registryItem in registry.EffectReg)
        {
            LoadedTexture texture = new(_api);
            api.Render.GetOrLoadTexture(new AssetLocation(registryItem.Value.EffectIconLocation()), ref texture);
            _textures[registryItem.Key] = texture;
            _langData[registryItem.Key] = registryItem.Value.EffectNameLocation();
        }
    }


    public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
    {
        double mousePosX = _api.Input.MouseX;
        double mousePosY = _api.Input.MouseY;

        int startX = 25;
        int offset = 8;

        int rendered = 0;
        int distanceApart = 100;

        foreach (KeyValuePair<string, EffectManager.DurationStrengthPair> currentEffect in _effectManager
                     .ClientSideEffectInfo)
        {
            _textures.TryGetValue(currentEffect.Key, out LoadedTexture texture);

            if (texture == null)
            {
                _api.Logger.Warning($"Effect {currentEffect.Key} has no valid texture!");
                continue;
            }

            _api.Render.Render2DTexturePremultipliedAlpha(_effectBackground.TextureId,
                startX + (rendered * distanceApart), 25, 80, 80);
            _api.Render.Render2DTexturePremultipliedAlpha(
                texture.TextureId,
                startX + (rendered * distanceApart) + offset, 33,
                64, 64
            );

            if (mousePosX > startX + (rendered * distanceApart) &&
                mousePosX < startX + (rendered * distanceApart) + 80 && mousePosY > 25 && mousePosY < 105)
            {
                TextTextureUtil textureUtil = new(_api);

                TextBackground textBackground = new();
                textBackground.FillColor = new[] { 0d, 0d, 0d, 0.5d };
                textBackground.Padding = 5;
                textBackground.Radius = 5;

                string effectName = Lang.Get(_langData[currentEffect.Key]);

                int nameSize = effectName.Length * 20;
                
                LoadedTexture durationText = textureUtil.GenTextTexture(
                    effectName + " " + currentEffect.Value.Strength + "\n" + currentEffect.Value.Duration + "s",
                    CairoFont.WhiteMediumText(), background: textBackground);
                _api.Render.Render2DTexturePremultipliedAlpha(durationText.TextureId,
                    mousePosX, mousePosY + 25, nameSize, 60);
                durationText.Dispose();
            }


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