using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Configurações")]
    public Transform Target;
    public Transform LookAt;
    public Vector3 RelativePosition;

    [Header("Camera Referência")]
    public Camera CameraSource;

    [Header("Presets de Resolução / Tamanho")]
    public ResolutionPreset DefaultPreset = ResolutionPreset.FHD_1080p;
    public List<ResolutionConfig> Presets = new List<ResolutionConfig>();
    private Camera _camera;

    private void Reset()
    {
        if (Presets == null || Presets.Count == 0)
        {
            Presets = GetDefaultPresets();
        }
    }

    private void OnValidate()
    {
        if (Presets == null || Presets.Count == 0)
        {
            Presets = GetDefaultPresets();
        }
    }

    void Awake()
    {
        // garante referência à câmera
        _camera = CameraSource != null ? CameraSource : GetComponent<Camera>();
        if (_camera == null)
        {
            Debug.LogWarning("CameraController: Nenhuma câmera encontrada no CameraSource nem no GameObject.");
        }

        // inicializa presets padrões caso lista esteja vazia (opcional)
        if (Presets == null || Presets.Count == 0)
        {
            Presets = GetDefaultPresets();
        }

        // aplica preset padrão ao iniciar
        SetResolutionPreset(DefaultPreset, setScreenResolution: false);

        // posiciona a câmera pela primeira vez se houver target
        if (Target != null)
        {
            transform.position = Target.position + RelativePosition;
        }
    }

    void LateUpdate()
    {
        // segue o alvo
        if (Target != null)
        {
            transform.position = Target.position + RelativePosition;
        }

        // olhe para o LookAt se definido
        if (LookAt != null)
        {
            transform.LookAt(LookAt);
        }
    }

    // Aplica um preset definido na lista (não altera resolução da janela por padrão)
    // setScreenResolution = true também chama Screen.SetResolution(width, height, Screen.fullScreen)
    public void SetResolutionPreset(ResolutionPreset preset, bool setScreenResolution = false)
    {
        var config = Presets.Find(p => p.Preset == preset);
        if (config == null)
        {
            Debug.LogWarning($"CameraController: Preset {preset} não encontrado.");
            return;
        }

        ApplyConfig(config, setScreenResolution);
    }

    // Aplica um tamanho ortográfico diretamente
    public void SetOrthographicSize(float size)
    {
        if (_camera == null) return;

        if (_camera.orthographic)
        {
            _camera.orthographicSize = Mathf.Max(0.01f, size);
        }
        else
        {
            Debug.LogWarning("SetOrthographicSize: A câmera não está em modo ortográfico.");
        }
    }

    // Aplica um config (interno)
    private void ApplyConfig(ResolutionConfig config, bool setScreenResolution)
    {
        if (_camera == null) return;

        if (config.IsOrthographic)
        {
            // aplica tamanho ortográfico
            _camera.orthographic = true;
            _camera.orthographicSize = config.OrthographicSize;
        }
        else
        {
            // aplica campo de visão para câmeras perspectiva
            _camera.orthographic = false;
            _camera.fieldOfView = config.PerspectiveFOV;
        }

        if (setScreenResolution && config.Width > 0 && config.Height > 0)
        {
            Screen.SetResolution(config.Width, config.Height, Screen.fullScreen);
        }
    }

    // Retorna presets padrão (HD, FHD, 4K) com valores razoáveis de orthographicSize / FOV.
    // Esses valores podem ser ajustados via inspector posteriormente.
    private List<ResolutionConfig> GetDefaultPresets()
    {
        return new List<ResolutionConfig>()
        {
            new ResolutionConfig()
            {
                Preset = ResolutionPreset.HD_720p,
                Width = 1280,
                Height = 720,
                IsOrthographic = true,
                OrthographicSize = 5f, // valor inicial sugerido (ajustar conforme cena / pixels per unit)
                PerspectiveFOV = 60f
            },
            new ResolutionConfig()
            {
                Preset = ResolutionPreset.FHD_1080p,
                Width = 1920,
                Height = 1080,
                IsOrthographic = true,
                OrthographicSize = 7f, // ajuste sugerido para FHD
                PerspectiveFOV = 60f
            },
            new ResolutionConfig()
            {
                Preset = ResolutionPreset.UHD_4K,
                Width = 3840,
                Height = 2160,
                IsOrthographic = true,
                OrthographicSize = 14f, // ajuste sugerido para 4K (depende de pixels-per-unit e cena)
                PerspectiveFOV = 60f
            },
            new ResolutionConfig()
            {
                Preset = ResolutionPreset.Custom,
                Width = 0,
                Height = 0,
                IsOrthographic = true,
                OrthographicSize = 7f,
                PerspectiveFOV = 60f
            }
        };
    }

    // Estrutura para configuração de cada preset (editável no Inspector)
    [System.Serializable]
    public class ResolutionConfig
    {
        public ResolutionPreset Preset;
        public int Width;
        public int Height;

        // Se true, usa OrthographicSize; se false, usa PerspectiveFOV
        public bool IsOrthographic = true;

        [Header("Orthographic")]
        public float OrthographicSize = 7f;

        [Header("Perspective")]
        public float PerspectiveFOV = 60f;
    }

    public enum ResolutionPreset
    {
        HD_720p,
        FHD_1080p,
        UHD_4K,
        Custom
    }
}
