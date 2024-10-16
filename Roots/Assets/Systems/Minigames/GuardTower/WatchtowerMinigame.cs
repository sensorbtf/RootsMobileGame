using System;
using System.Collections;
using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Minigames
{
    public class WatchtowerMinigame : Minigame
    {
        [SerializeField] private Sprite _stormBackground;
        [SerializeField] private Sprite _sunBackground;
        [SerializeField] private Image _background;
        [SerializeField] private GameObject _topLayer;
        [SerializeField] private Texture2D _topTexture;
        [SerializeField] private int _brushSize;
        [SerializeField] private LocalizedString _failed;
        private bool _erasedEverything = false;

        private new void Update()
        {
            if (!_isGameActive)
                return;
            
            base.Update();
            
            var pos = Vector2.zero;
            var inputDetected = false;

            if (Input.GetMouseButton(0))
            {
                pos = Input.mousePosition;
                inputDetected = true;
            }
            else if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    pos = touch.position;
                    inputDetected = true;
                }
            }

            if (inputDetected)
            {
                Vector2 texturePos = GetInputPosition(pos);
                EraseAtPosition(texturePos);
            }

            if (_erasedEverything)
            {
                _timer = 0;
                base.Update();
                _isGameActive = false;
                _topLayer.SetActive(false);
                _collectPointsButton.interactable = true;
                return;
            }
            
            if (_timer <= 0)
            {
                _timer = 0;
                _isGameActive = false;
                _collectPointsButton.interactable = true;
                _timeText.text = _failed.GetLocalizedString();
                return;
            }
        }
        
        private IEnumerator CheckErasureCompletion()
        {
            while (true)
            {
                yield return new WaitForSeconds(1); 

                if (IsEverythingErased())
                {
                    _erasedEverything = true;
                    break;
                }
            }
        }

        public override void SetupGame(Building p_building)
        {
            base.SetupGame(p_building);

            if (_worldManager.WillStormBeInTwoDays())
            {
                _background.sprite = _stormBackground;
            }
            else
            {
                _background.sprite = _sunBackground;
            }
            
            Image imageComponent = _topLayer.GetComponent<Image>();
            Sprite sprite = imageComponent.sprite;
            _topTexture = CreateReadableTexture(sprite.texture);

            _erasedEverything = false;
            _score = 0;

            _collectPointsButton.interactable = false;
        }

        private Vector2 GetInputPosition(Vector2 p_inputPos)
        {
            RectTransform rectTransform = _topLayer.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, p_inputPos, null,
                out Vector2 localPoint);

            var rect = rectTransform.rect;
            Vector2 texturePoint = new Vector2(
                (localPoint.x + rect.width * 0.5f) / rect.width,
                (localPoint.y + rect.height * 0.5f) / rect.height
            );

            return texturePoint;
        }
        
        private bool IsEverythingErased()
        {
            var pixels = _topTexture.GetPixels();
            var transparentPixelCount = 0;
            var totalPixelCount = pixels.Length;

            foreach (var pixel in pixels)
            {
                if (pixel.a == 0) 
                {
                    transparentPixelCount++;
                }
            }

            var transparentPercentage = (float)transparentPixelCount / totalPixelCount * 100f;
    
            return transparentPercentage >= 80f;
        }

        private void EraseAtPosition(Vector2 p_position)
        {
            float radiusSquared = _brushSize * _brushSize * _efficiency;

            Color[] pixels = _topTexture.GetPixels();

            for (int i = -_brushSize; i <= _brushSize; i++)
            {
                for (int j = -_brushSize; j <= _brushSize; j++)
                {
                    if (!(i * i + j * j <= radiusSquared)) 
                        continue;
                    
                    var px = Mathf.Clamp((int)(p_position.x * _topTexture.width) + i, 0, _topTexture.width - 1);
                    var py = Mathf.Clamp((int)(p_position.y * _topTexture.height) + j, 0, _topTexture.height - 1);
                    pixels[py * _topTexture.width + px] = new Color(0, 0, 0, 0);
                }
            }

            _topTexture.SetPixels(pixels);
            _topTexture.Apply();
            ApplyTextureToUI(_topTexture);
        }
        
        private void ApplyTextureToUI(Texture2D texture)
        {
            if (_topLayer.GetComponent<Image>() != null)
            {
                Sprite newSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                _topLayer.GetComponent<Image>().sprite = newSprite;
            }
        }

        public override void AddScore()
        {
        }

        public override void StartMinigame()
        {
            StartCoroutine(CheckErasureCompletion());
        }
        
        Texture2D CreateReadableTexture(Texture2D original)
        {
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(original.width, original.height);
            Graphics.Blit(original, renderTexture);

            RenderTexture.active = renderTexture;
            Texture2D readableTexture = new Texture2D(original.width, original.height);
            readableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(renderTexture);

            return readableTexture;
        }
    }
}