using GTA;
using GTA.Native;
using GTA.Math;

namespace ForcedFirstPersonAim
{
    public class AimCamMonitor : Script
    {
        int prevMode;
        bool hasAimed;
        Camera startCam, endCam;
        Timer focusEntTimer, focusExTimer;

        public AimCamMonitor()
        {
            this.Tick += (sender, args) => OnTick();
            focusEntTimer = new Timer(175);
            focusExTimer = new Timer(5);
            startCam = null;
            endCam = null;
        }

        private void OnTick()
        {
            if (Function.Call<bool>(Hash.IS_CUTSCENE_PLAYING)) return;

            if (endCam != null && endCam.IsActive && !endCam.IsInterpolating)
                World.RenderingCamera = null;

            var player = Game.Player.Character;

            if (!player.IsOnFoot ||
                player.Weapons.Current == null ||
                Function.Call<int>(Hash.GET_WEAPON_CLIP_SIZE, (int)player.Weapons.Current.Hash) == 0)
                return;

            if (Function.Call<bool>(Hash.IS_PLAYER_FREE_AIMING, Game.Player.Handle))
            {
                if (Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE) != 4 && (!focusEntTimer.Enabled && !hasAimed))
                {
                    var camRot = GameplayCamera.Rotation;
                    var startPos = GameplayCamera.Position;
                    var endPos = (Game.Player.Character.IsRunning ||
                        player.IsSprinting) ? 
                        player.GetOffsetInWorldCoords(new Vector3(-0.009f, 0.07f, 0.60f)) :
                        player.GetOffsetInWorldCoords(new Vector3(-0.002f, 0.07f, 0.63f));

                    startCam = new Camera(Function.Call<int>(Hash.CREATE_CAM_WITH_PARAMS, "default_scripted_camera", startPos.X, startPos.Y, startPos.Z, camRot.X, camRot.Y, camRot.Z, GameplayCamera.FieldOfView, 0, 2));
                    endCam = new Camera(Function.Call<int>(Hash.CREATE_CAM_WITH_PARAMS, "default_scripted_camera", endPos.X, endPos.Y, endPos.Z, camRot.X, camRot.Y, camRot.Z, GameplayCamera.FieldOfView, 0, 2));

                    Function.Call(Hash.SET_CAM_ACTIVE_WITH_INTERP, endCam.Handle, startCam.Handle, 180, 0, 1);
                    Function.Call(Hash.RENDER_SCRIPT_CAMS, 1, 0, 3000, 1, 0, 0);
                    Function.Call(Hash._0x48621C9FCA3EBD28, 4);

                    prevMode = Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE);
                    focusEntTimer.Start();
                    hasAimed = true;
                }
            }

            else
            {
                if (hasAimed)
                {
                    if (!focusExTimer.Enabled)
                        focusExTimer.Start();
                    hasAimed = false;
                }

                if (focusExTimer.Enabled && Game.GameTime > focusExTimer.Waiter)
                {
                    focusExTimer.Enabled = false;
                    Function.Call(Hash.SET_FOLLOW_PED_CAM_VIEW_MODE, prevMode);
                    startCam?.Destroy();
                    endCam?.Destroy();
                    World.RenderingCamera = null;
                }
            }

            if (focusEntTimer.Enabled && Game.GameTime > focusEntTimer.Waiter)
            {
                focusEntTimer.Enabled = false;            
                if (Function.Call<bool>(Hash.IS_PLAYER_FREE_AIMING, Game.Player.Handle))
                    Function.Call(Hash.SET_FOLLOW_PED_CAM_VIEW_MODE, 4);
            }
        }
    }
}
