using System.Collections;
using UnityEngine;

public class RandomNumberChecker : MonoBehaviour
{
    private bool isOne = false;
    public MeshRenderer cubeRenderer;

    // Start is called before the first frame update
    void Start()
    {
        // 큐브의 Renderer 컴포넌트를 가져옵니다.
        cubeRenderer = GetComponent<MeshRenderer>();

        // 코루틴 시작
        StartCoroutine(CheckRandomNumber());
    }

    // 0.5초마다 랜덤 숫자 체크
    private IEnumerator CheckRandomNumber()
    {
        while (true)
        {
            // 1부터 100까지 랜덤한 숫자 생성
            int randomNumber = Random.Range(1, 101);

            // 랜덤 숫자가 1이면 isOne을 true로 설정
            if (randomNumber == 1)
            {
                isOne = true;
                Debug.Log("True! The number is 1.");
            }
            else
            {
                isOne = false;
            }

            // isOne이 true일 때 큐브를 보이게, false일 때 숨기기
            cubeRenderer.enabled = isOne;

            // 0.5초 대기
            yield return new WaitForSeconds(0.5f);
        }
    }

    // 상태 출력
    public bool IsOne()
    {
        return isOne;
    }
}
