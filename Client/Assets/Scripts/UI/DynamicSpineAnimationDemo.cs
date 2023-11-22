using System.IO;
using System.Linq;
using System.Net;
using Cysharp.Threading.Tasks;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Noobie.SanGuoSha.UI
{
    public class DynamicSpineAnimationDemo : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            CreateSpineAsync().Forget();
        }

        async UniTaskVoid CreateSpineAsync()
        {
            var texturePath = Path.Combine(Application.streamingAssetsPath, "Skins/LiuYan/01/dynamic/XingXiang.png");
            var request = UnityWebRequestTexture.GetTexture(texturePath);
            await request.SendWebRequest();
            if (request.responseCode != (int)HttpStatusCode.OK) return;

            var texture = DownloadHandlerTexture.GetContent(request);
            texture.name = "XingXiang";

            request = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, "Skins/LiuYan/01/dynamic/XingXiang.atlas"));
            await request.SendWebRequest();
            if (request.responseCode != (int)HttpStatusCode.OK) return;

            var atlas = new TextAsset(request.downloadHandler.text);

            request = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, "Skins/LiuYan/01/dynamic/XingXiang.skel"));
            await request.SendWebRequest();
            if (request.responseCode != (int)HttpStatusCode.OK) return;

            var material = Resources.Load<Material>("UI/SpineSkeletonPropertySource");
            var saa = SpineAtlasAsset.CreateRuntimeInstance(atlas, new[] { texture }, material, true);
            var sda = SkeletonDataAsset.CreateRuntimeInstance(new TextAsset { name = "XingXiang.skel" }, saa, false);
            sda.SetOverwriteBinaryData(request.downloadHandler.data);
            sda.GetSkeletonData(false);

            var sa = SkeletonAnimation.NewSkeletonAnimationGameObject(sda);

            sa.transform.SetParent(transform, false);

            sa.AnimationName = sa.Skeleton.Data.Animations.First()?.Name;

            void OnAnimationComplete(TrackEntry entry)
            {
                Debug.Log($"animation complete, {entry.Animation.Name}");

                sa.loop = true;
                sa.AnimationName = sa.Skeleton.Data.Animations.ElementAt(1).Name;
                sa.AnimationState.Complete -= OnAnimationComplete;
            }

            sa.AnimationState.Complete += OnAnimationComplete;
        }
    }

}