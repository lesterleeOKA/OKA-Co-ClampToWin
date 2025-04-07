using SimpleJSON;
using System;

[Serializable]
public class GameSettings : Settings
{
    public int playerNumber = 0;
    public string grid_image;
    public string clamp_open_image, clamp_clamped_image;
    public float clamp_extend_speed = 0.25f;
    public float clamp_rotation_speed = 30f;
}

public static class SetParams
{
    public static void setCustomParameters(GameSettings settings = null, JSONNode jsonNode= null)
    {
        if (settings != null && jsonNode != null)
        {
            ////////Game Customization params/////////
            string grid_image = jsonNode["setting"]["grid_image"] != null ?
                jsonNode["setting"]["grid_image"].ToString().Replace("\"", "") : null;

            string _clamp_open_image = jsonNode["setting"]["clamp_open"] != null ?
                jsonNode["setting"]["clamp_open"].ToString().Replace("\"", "") : null;

            string _clamp_clamped_image = jsonNode["setting"]["clamp_clamped"] != null ?
                jsonNode["setting"]["clamp_clamped"].ToString().Replace("\"", "") : null;

            settings.playerNumber = jsonNode["setting"]["player_number"] != null ? jsonNode["setting"]["player_number"] : 4;
            settings.clamp_extend_speed = jsonNode["setting"]["clamp_extend_speed"] != null ? jsonNode["setting"]["clamp_extend_speed"] : 0.25f;
            settings.clamp_rotation_speed = jsonNode["setting"]["clamp_rotation_speed"] != null ? jsonNode["setting"]["clamp_rotation_speed"] : 30f;

            LoaderConfig.Instance.gameSetup.playerNumber = settings.playerNumber;
            LoaderConfig.Instance.gameSetup.playersClampSpeed = settings.clamp_extend_speed;
            LoaderConfig.Instance.gameSetup.playersRotationSpeed = settings.clamp_rotation_speed;


            if (grid_image != null)
            {
                if (!grid_image.StartsWith("https://") || !grid_image.StartsWith(APIConstant.blobServerRelativePath))
                    settings.grid_image = APIConstant.blobServerRelativePath + grid_image;
            }

            if (_clamp_open_image != null)
            {
                if (!_clamp_open_image.StartsWith("https://") || !_clamp_open_image.StartsWith(APIConstant.blobServerRelativePath))
                    settings.clamp_open_image = APIConstant.blobServerRelativePath + _clamp_open_image;
            }

            if (_clamp_clamped_image != null)
            {
                if (!_clamp_clamped_image.StartsWith("https://") || !_clamp_clamped_image.StartsWith(APIConstant.blobServerRelativePath))
                    settings.clamp_clamped_image = APIConstant.blobServerRelativePath + _clamp_clamped_image;
            }
        }
    }
}

