using System;
using System.Collections;
using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames
{
    public class WatchtowerRightLeftClickingMinigame : Minigame
    {
        [SerializeField] private GameObject _topLayer;
        [SerializeField] private Texture2D _topTexture;
        private bool _isErasing = false;
        private bool _erasedEverything = false;

        private new void Update()
        {
            if (!_isErasing)
                return;
            
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
                _isGameActive = false;
                _topLayer.SetActive(false);
                _collectPointsButton.interactable = true;

                _timeText.text = "Check storm in 2 days";
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

            Image imageComponent = _topLayer.GetComponent<Image>();
            Sprite sprite = imageComponent.sprite;
            _topTexture = CreateReadableTexture(sprite.texture);

            _isErasing = false;
            _erasedEverything = false;
            _score = 0;

            _collectPointsButton.interactable = false;
            
            _timeText.text = $"Blur out panel";
            _scoreText.text = $"Reveal Horizon";
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
            int x = (int)(p_position.x * _topTexture.width);
            int y = (int)(p_position.y * _topTexture.height);
            int brushSize = 50;
            float radiusSquared = brushSize * brushSize;

            for (int i = -brushSize; i <= brushSize; i++)
            {
                for (int j = -brushSize; j <= brushSize; j++)
                {
                    if (i * i + j * j <= radiusSquared)
                    {
                        int px = Mathf.Clamp(x + i, 0, _topTexture.width - 1);
                        int py = Mathf.Clamp(y + j, 0, _topTexture.height - 1);
                        _topTexture.SetPixel(px, py, new Color(0, 0, 0, 0));
                    }
                }
            }

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
            _score += _efficiency;
            StartMinigame();
            _scoreText.text = $"Score: {_score:F0}";
        }

        public override void StartMinigame()
        {
            _isErasing = true;
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