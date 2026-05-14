using System;
using System.Collections;
using RTS.EventSystem;
using RTS.UI.Components;
using RTS.Units;
using UnityEngine;

namespace RTS.UI.Containers
{
    public class BuildingsBuildingUI : MonoBehaviour, IUIElement<CommandPost>
    {
        [SerializeField] private ProgressBar progressBar;
        private CommandPost commandPost;
        private Coroutine BuildingCoroutine;

        public void EnableFor(CommandPost item)
        {
            gameObject.SetActive(true);
            commandPost = item;

            commandPost.OnQueneUpdate += HandleQueneUIUpdate;
            //Debug.Log("1");
            BuildingCoroutine = StartCoroutine(UpdateUnitProgress());
        }
        public void Disable()
        {
            if (commandPost != null)
            {
                commandPost.OnQueneUpdate -= HandleQueneUIUpdate;
            }
            gameObject.SetActive(false);
            commandPost = null;
        }

        private void HandleQueneUIUpdate(UnitSO[] unitQuene)
        {
            if (unitQuene.Length >= 1 && BuildingCoroutine != null)
            {
                Debug.Log("开始更新建造进度1");
                BuildingCoroutine = StartCoroutine(UpdateUnitProgress());
            }
            else if (unitQuene.Length >= 1)
            {
                BuildingCoroutine = StartCoroutine(UpdateUnitProgress());
            }

        }
        // 更新建造进度条
        private IEnumerator UpdateUnitProgress()
        {

            while (commandPost != null && commandPost.QueneSize > 0)
            {
                Debug.Log("开始更新建造进度2");
                float startTime = commandPost.CurrentQueneStartTime;
                float BuildTime = commandPost.CurrentBuildingUnit.BuildTime;

                float progress = Mathf.Clamp01((Time.time - startTime) / BuildTime);
                progressBar.SetProgress(progress);
                yield return null;
            }
        }
    }

}