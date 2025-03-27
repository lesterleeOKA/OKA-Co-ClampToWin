using SimpleJSON;
using System;

[Serializable]
public class GameSettings : Settings
{
    public int playerNumber = 0;
    public string grid_image;
    public float playersClampSpeed = 3f;
    public float playersRotationSpeed = 200f;
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

            settings.playerNumber = jsonNode["setting"]["player_number"] != null ? jsonNode["setting"]["player_number"] : 4;
            settings.playersClampSpeed = jsonNode["setting"]["clamp_moving_speed"] != null ? jsonNode["setting"]["clamp_moving_speed"] : 3f;
            settings.playersRotationSpeed = jsonNode["setting"]["rotation_speed"] != null ? jsonNode["setting"]["rotation_speed"] : 200f;

            LoaderConfig.Instance.gameSetup.playerNumber = settings.playerNumber;
            LoaderConfig.Instance.gameSetup.playersClampSpeed = settings.playersClampSpeed;
            LoaderConfig.Instance.gameSetup.playersRotationSpeed = settings.playersRotationSpeed;


            if (grid_image != null)
            {
                if (!grid_image.StartsWith("https://") || !grid_image.StartsWith(APIConstant.blobServerRelativePath))
                    settings.grid_image = APIConstant.blobServerRelativePath + grid_image;
            }
        }
    }
}

