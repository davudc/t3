﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace T3.Core.Operator.Types
{
    public class LoadTexture2d : Instance<LoadTexture2d>
    {
        //[Output(Guid = "{E0C4FEDD-5C2F-46C8-B67D-5667435FB037}")]
        //public readonly Slot<Texture2D> Texture = new Slot<Texture2D>();

        [Output(Guid = "{D45D32B6-E588-4723-A071-6E02406B831A}")]
        public readonly Slot<ShaderResourceView> ShaderResourceView = new Slot<ShaderResourceView>();

        private Texture2D _texture;
        private Guid _textureResId;
        private bool _textureChanged = true;
        private Guid _textureSrvResId;

        public LoadTexture2d()
        {
            //Texture.UpdateAction = UpdateTexture;
            ShaderResourceView.UpdateAction = UpdateShaderResourceView;
        }

        private void UpdateTexture(EvaluationContext context)
        {
            if (_textureChanged)
            {
                string imagePath = Path.GetValue(context);
                (Guid textureResId, Guid textureSrvResId) = ResourceManager.Instance().CreateTextureFromFile(imagePath);
                _textureResId = textureResId;
                _textureSrvResId = textureSrvResId;
            }
        }

        private void UpdateShaderResourceView(EvaluationContext context)
        {
            UpdateTexture(context);

            if (ResourceManager.Instance().Resources[_textureSrvResId] is ShaderResourceViewResource srvr)
                ShaderResourceView.Value = srvr.ShaderResourceView;

            _textureChanged = false;
        }

        [Input(Guid = "{76CC3811-4AE0-48B2-A119-890DB5A4EEB2}")]
        public readonly InputSlot<string> Path = new InputSlot<string>();
    }
}