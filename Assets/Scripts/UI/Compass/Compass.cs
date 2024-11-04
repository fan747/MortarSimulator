using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    [SerializeField] private RawImage _compassImage;
    [SerializeField] private Transform _mortarTransform;
    [SerializeField] private Transform _enemyBaseTransform;
    [SerializeField] private RectTransform _compassBar;
    [SerializeField] private RectTransform _enemyBaseRect;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Text _distanceAfterEnemyText;

    private const float withdCompassImageDivider = 67.5f;
    private float _compassUnit;

    private void Start()
    {
        _compassUnit = _compassImage.rectTransform.rect.width / withdCompassImageDivider;
    }

    private void Update()
    {
        _compassImage.uvRect = new Rect(_mainCamera.gameObject.transform.localEulerAngles.y / 360, 0, 1, 1);
        SetMarkerPosition(_enemyBaseTransform.position, _enemyBaseRect);
    }

    private void SetMarkerPosition(Vector3 worldPosition, RectTransform rectTransform)
    {
        // Вычисляем направление к базе врага
        Vector3 directionToTarget = worldPosition - _mainCamera.gameObject.transform.position;
        _distanceAfterEnemyText.text = $"{Mathf.RoundToInt(Vector3.Distance(_mortarTransform.position, _enemyBaseTransform.position))}m";

        // Вычисляем угол между направлением вперед игрока и направлением к базе врага
        float angle = Vector2.SignedAngle(new Vector2(directionToTarget.x, directionToTarget.z), new Vector2(_mainCamera.gameObject.transform.forward.x, _mainCamera.gameObject.transform.forward.z));

        rectTransform.anchoredPosition = new Vector2(_compassUnit * angle, rectTransform.anchoredPosition.y);
    }
}
