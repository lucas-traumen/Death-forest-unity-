using UnityEngine;

namespace HollowManor
{
    public sealed class CarEscapeInteractable : Interactable
    {
        public override string GetPrompt(PlayerInteractor interactor)
        {
            if (GameManager.Instance == null)
            {
                return string.Empty;
            }

            if (GameManager.Instance.CarRepaired)
            {
                return "E - Lao lên xe và nổ máy";
            }

            return GameManager.Instance.CanRepairCar(out string reason)
                ? "E - Lắp linh kiện, sửa xe và thoát"
                : reason;
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            if (!GameManager.Instance.CarRepaired)
            {
                if (!GameManager.Instance.CanRepairCar(out string reason))
                {
                    GameManager.Instance.ShowToast(reason);
                    return;
                }

                GameManager.Instance.RepairCar();
            }

            if (!GameManager.Instance.CarRepaired)
            {
                return;
            }

            EndingSequencePoints authoredPoints = EndingSequencePoints.FindForTransform(transform);
            if (authoredPoints != null)
            {
                GameManager.Instance.BeginCarEscapeSequence(authoredPoints);
                return;
            }

            Vector3 doorPosition = transform.position + new Vector3(-0.45f, -0.35f, 0f);
            Vector3 seatPosition = transform.position + new Vector3(0.72f, 0.18f, -0.18f);
            Quaternion seatRotation = Quaternion.Euler(0f, 180f, 0f);
            GameManager.Instance.BeginCarEscapeSequence(doorPosition, seatPosition, seatRotation);
        }
    }
}
