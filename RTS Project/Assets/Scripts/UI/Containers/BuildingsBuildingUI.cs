using System;
using System.Collections;
using RTS.EventSystem;
using RTS.UI.Components;
using RTS.Units;
using UnityEngine;

namespace RTS.UI.Containers
{
    public class BuildingsBuildingUI : MonoBehaviour, IUIElement<CommandableBuilding>
    {
        [SerializeField] private UIBuildQueneButton[] UIBuildingQueneButtons;
        [SerializeField] private ProgressBar progressBar;
        private CommandableBuilding CommandableBuilding;
        private Coroutine BuildingCoroutine;

        public void EnableFor(CommandableBuilding item)
        {
            gameObject.SetActive(true);
            CommandableBuilding = item;

            CommandableBuilding.OnQueneUpdate += HandleQueneUIUpdate;
            //Debug.Log("1");
            BuildingCoroutine = StartCoroutine(UpdateUnitProgress());
        }
        public void Disable()
        {
            if (CommandableBuilding != null)
            {
                CommandableBuilding.OnQueneUpdate -= HandleQueneUIUpdate;
            }
            gameObject.SetActive(false);
            CommandableBuilding = null;
            BuildingCoroutine = null;
        }
        private void HandleQueneButtonUpdate()
        {
            int i = 0;
            for (; i < CommandableBuilding.QueneSize; i++)
            {
                int index = i;// 防闭包的异常捕获
                UIBuildingQueneButtons[i].EnableFor(CommandableBuilding.Quene[i]
                ,() => CommandableBuilding.CancelBuildingUnit(index));
            }
            for (; i < UIBuildingQueneButtons.Length; i++)
            {
                UIBuildingQueneButtons[i].Disable();
            }
        }

        private void HandleQueneUIUpdate(UnitSO[] unitQuene)
        {
            if (unitQuene.Length == 1 && BuildingCoroutine == null)
            {
                Debug.Log("开始更新建造进度1");
                BuildingCoroutine = StartCoroutine(UpdateUnitProgress());
            }
            HandleQueneButtonUpdate();

        }
        // 更新建造进度条
        private IEnumerator UpdateUnitProgress()
        {

            while (CommandableBuilding != null && CommandableBuilding.QueneSize > 0)
            {
                Debug.Log("开始更新建造进度2");
                float startTime = CommandableBuilding.CurrentQueneStartTime;
                float BuildTime = CommandableBuilding.CurrentBuildingUnit.BuildTime;

                float progress = Mathf.Clamp01((Time.time - startTime) / BuildTime);
                progressBar.SetProgress(progress);
                yield return null;
            }
            BuildingCoroutine = null;
        }

    }

}