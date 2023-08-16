using UnityEngine;
using UnityEngine.UI;

namespace Invector.vCharacterController
{
    public class vHUDController : MonoBehaviour
    {
        #region General Variables

        #region Health/Stamina Variables
        [Header("Health/Stamina")]
        public Slider healthSlider;
        public float healthSliderMaxValueSmooth = 10f;
        public float healthSliderValueSmooth = 10;
        public Slider staminaSlider;
        public float staminaSliderMaxValueSmooth = 10f;
        public float staminaSliderValueSmooth = 100;
        [Header("DamageHUD")]
        public Image damageImage;
        public float flashSpeed = 5f;
        public Color flashColour = new Color(1f, 0f, 0f, 0.1f);
        [HideInInspector] public bool damaged;
        #endregion

        #region Display Controls Variables
        [Header("Controls Display")]
        [HideInInspector]
        public bool controllerInput;
        public Image displayControls;
        public Sprite joystickControls;
        public Sprite keyboardControls;
        #endregion

        #region Debug Info Variables
        [Header("Debug Window")]
        public GameObject debugPanel;
        [HideInInspector]
        public Text debugText;
        #endregion

        #region Change Input Text Variables
        [Header("Text with FadeIn/Out")]
        public Text fadeText;
        private float textDuration, fadeDuration, durationTimer, timer;
        private Color startColor, endColor;
        private bool fade;
        #endregion

        #endregion

        private static vHUDController _instance;
        public static vHUDController instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<vHUDController>();
                    //Tell unity not to destroy this object when loading a new scene
                    //DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }

        void Start()
        {
            InitFadeText();
            if (debugPanel != null)
                debugText = debugPanel.GetComponentInChildren<Text>();
        }

        public void Init(vThirdPersonController cc)
        {
            cc.onDead.AddListener(OnDead);
            cc.onReceiveDamage.AddListener(EnableDamageSprite);
            damageImage.color = new Color(0f, 0f, 0f, 0f);
            if (healthSlider)
            {
                if (cc.maxHealth != healthSlider.maxValue)
                {
                    healthSlider.maxValue = cc.maxHealth;
                    healthSlider.onValueChanged.Invoke(healthSlider.value);
                }
                healthSlider.value = cc.currentHealth;
            }
            if (staminaSlider)
            {
                if (cc.maxStamina != staminaSlider.maxValue)
                {
                    staminaSlider.maxValue = cc.maxStamina;
                    staminaSlider.onValueChanged.Invoke(staminaSlider.value);
                }
                staminaSlider.value = cc.currentStamina;
            }
        }

        private void OnDead(GameObject arg0)
        {
            ShowText("You are Dead!");
        }

        public virtual void UpdateHUD(vThirdPersonController cc)
        {
            UpdateDebugWindow(cc);
            UpdateSliders(cc);
            ChangeInputDisplay();
            ShowDamageSprite();
            FadeEffect();
        }

        public void ShowText(string message, float textTime = 2f, float fadeTime = 0.5f)
        {
            if (fadeText != null && !fade)
            {
                fadeText.text = message;
                textDuration = textTime;
                fadeDuration = fadeTime;
                durationTimer = 0f;
                timer = 0f;
                fade = true;
            }
            else if (fadeText != null)
            {
                fadeText.text += "\n" + message;
                textDuration = textTime;
                fadeDuration = fadeTime;
                durationTimer = 0f;
                timer = 0f;
            }
        }

        public void ShowText(string message)
        {
            if (fadeText != null && !fade)
            {
                fadeText.text = message;
                textDuration = 2f;
                fadeDuration = 0.5f;
                durationTimer = 0f;
                timer = 0f;
                fade = true;
            }
            else if (fadeText != null)
            {
                fadeText.text += "\n" + message;
                textDuration = 2f;
                fadeDuration = 0.5f;
                durationTimer = 0f;
                timer = 0f;
            }
        }

        void UpdateSliders(vThirdPersonController cc)
        {
            if (healthSlider != null)
            {
                if (cc.maxHealth != healthSlider.maxValue)
                {
                    healthSlider.maxValue = Mathf.Lerp(healthSlider.maxValue, cc.maxHealth, healthSliderMaxValueSmooth * Time.fixedDeltaTime);
                    healthSlider.onValueChanged.Invoke(healthSlider.value);
                }
                healthSlider.value = Mathf.Lerp(healthSlider.value, cc.currentHealth, healthSliderValueSmooth * Time.fixedDeltaTime);
            }
            if (staminaSlider)
            {
                if (cc.maxStamina != staminaSlider.maxValue)
                {
                    staminaSlider.maxValue = Mathf.Lerp(staminaSlider.maxValue, cc.maxStamina, staminaSliderMaxValueSmooth * Time.fixedDeltaTime);
                    staminaSlider.onValueChanged.Invoke(staminaSlider.value);
                }
                staminaSlider.value = Mathf.Lerp(staminaSlider.value, cc.currentStamina, staminaSliderValueSmooth * Time.fixedDeltaTime);
            }
        }

        public void ShowDamageSprite()
        {
            if (damaged)
            {
                damaged = false;
                if (damageImage != null)
                    damageImage.color = flashColour;
            }
            else if (damageImage != null)
                damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }

        public void EnableDamageSprite(vDamage damage)
        {
            if (damageImage != null)
                damageImage.enabled = true;
            damaged = true;
        }

        void UpdateDebugWindow(vThirdPersonController cc)
        {
            if (cc.debugWindow)
            {
                if (debugPanel != null && !debugPanel.activeSelf)
                    debugPanel.SetActive(true);
                if (debugText)
                    debugText.text = cc.DebugInfo();
            }
            else
            {
                if (debugPanel != null && debugPanel.activeSelf)
                    debugPanel.SetActive(false);
            }
        }

        void ChangeInputDisplay()
        {
            if (displayControls == null) return;
#if MOBILE_INPUT
		displayControls.enabled = false;
#else
            if (controllerInput)
                displayControls.sprite = joystickControls;
            else
                displayControls.sprite = keyboardControls;
#endif
        }

        void InitFadeText()
        {
            if (fadeText != null)
            {
                fadeText.verticalOverflow = VerticalWrapMode.Overflow;
                startColor = fadeText.color;
                endColor.a = 0f;
                fadeText.color = endColor;
            }
            else
                Debug.Log("Please assign a Text object on the field Fade Text");
        }

        void FadeEffect()
        {
            if (fadeText != null)
            {
                if (fade)
                {
                    fadeText.color = Color.Lerp(endColor, startColor, timer);

                    if (timer < 1)
                        timer += Time.deltaTime / fadeDuration;

                    if (fadeText.color.a >= 1)
                    {
                        fade = false;
                        timer = 0f;
                    }
                }
                else
                {
                    if (fadeText.color.a >= 1)
                        durationTimer += Time.deltaTime;

                    if (durationTimer >= textDuration)
                    {
                        fadeText.color = Color.Lerp(startColor, endColor, timer);
                        if (timer < 1)
                            timer += Time.deltaTime / fadeDuration;
                    }
                }
            }
        }
    }
}