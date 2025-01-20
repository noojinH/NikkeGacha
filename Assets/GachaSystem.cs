using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스 추가
using UnityEngine.UI; // UI 네임스페이스 추가
using System;

public class GachaSystem : MonoBehaviour
{
    // 확률 설정
    private const double PICKUP_RATE = 0.01; // 1%
    private const double SSR_RATE = 0.03;    // 3%
    private const double SR_RATE = 0.43;     // 43%
    private const double R_RATE = 0.53;      // 53%
    private const int JEWEL_COST_PER_PULL = 300; // 1뽑당 300쥬얼
    private int ticketCount;

    // UI 요소
    public TMP_Text resultText; // TMP 텍스트 (결과 출력용)
    public Button gachaButton;  // 버튼
    public Button gachaButton1;  // 버튼
    public Button expectButton;
    public Button pickButton;
    public TMP_InputField ticketInputField;  // 티켓 입력 필드 추가
    public TMP_InputField ticketInputField1; // 고급 티켓 입력 필드
public TMP_InputField jewelInputField;  // 쥬얼 입력 필드
public TMP_Text probabilityText;         // 확률 출력 텍스트
    public Color yellowColor = new Color(1f, 0.84f, 0f);  // 노란색
    public Color blueColor = new Color(0f, 0f, 1f);       // 파란색
    public Color purpleColor = new Color(0.5f, 0f, 0.5f); // 보라색

    void Start()
    {
        // 버튼에 클릭 이벤트 연결
        gachaButton1.onClick.AddListener(aGachaButtonClicked);
        gachaButton.onClick.AddListener(tenGachaButtonClicked);
        expectButton.onClick.AddListener(OnexpectButtonClicked);
        pickButton.onClick.AddListener(OnCalculateProbabilityClicked);
    }

    void aGachaButtonClicked(){
        string rs = PerformGacha(1, PICKUP_RATE, SSR_RATE, SR_RATE, R_RATE)[0];
        if (rs.Contains("SSR"))
        {
            StartCoroutine(ChangeButtonColorAndDisable(yellowColor, gachaButton1));
        }
        else if(rs.Contains("SR"))
        {
            // SSR 캐릭터가 없을 경우에만 랜덤 색상 적용
            StartCoroutine(ChangeButtonColorAndDisable(purpleColor, gachaButton1));
        }
        else{
            StartCoroutine(ChangeButtonColorAndDisable(blueColor, gachaButton1));
        }

        // 결과를 TMP 텍스트로 표시 (등급별 색상 적용)
        resultText.text = "결과:\n";
        string ft = "";
        
        if (rs.Contains("픽업 캐릭터") || rs.Contains("SSR"))
            {
                ft += $"<color=#FFD700>{rs}</color>\n"; // 노란색
            }
            else if (rs.Contains("SR"))
            {
                ft += $"<color=#800080>{rs}</color>\n"; // 보라색
            }
            else if (rs.Contains("R"))
            {
                ft += $"<color=#0000FF>{rs}</color>\n"; // 파란색
        }
        resultText.text = resultText.text + ft;
    }

    void tenGachaButtonClicked(){
        // 10연챠 결과 얻기
        List<string> results = PerformGacha(10, PICKUP_RATE, SSR_RATE, SR_RATE, R_RATE);

        // SSR 캐릭터가 나온 경우에는 노란색 버튼으로 설정
        if (results.Exists(result => result.Contains("SSR")))
        {
            StartCoroutine(ChangeButtonColorAndDisable(yellowColor, gachaButton));
        }
        else
        {
            // SSR 캐릭터가 없을 경우에만 랜덤 색상 적용
            StartCoroutine(ChangeButtonColorAndDisable(GetRandomColor(), gachaButton));
        }

        // 결과를 TMP 텍스트로 표시 (등급별 색상 적용)
        resultText.text = "결과:\n" + FormatResultsWithColors(results);
    }

    string FormatResultsWithColors(List<string> results)
    {
        // TMP 색상 태그를 사용하여 결과 텍스트를 포맷
        string formattedResults = "";
        foreach (string result in results)
        {
            if (result.Contains("픽업 캐릭터") || result.Contains("SSR"))
            {
                formattedResults += $"<color=#FFD700>{result}</color>\n"; // 노란색
            }
            else if (result.Contains("SR"))
            {
                formattedResults += $"<color=#800080>{result}</color>\n"; // 보라색
            }
            else if (result.Contains("R"))
            {
                formattedResults += $"<color=#0000FF>{result}</color>\n"; // 파란색
            }
        }
        return formattedResults;
    }

    void OnCalculateProbabilityClicked()
{
    // 입력된 고급 티켓과 쥬얼 개수 가져오기
    int tickets1 = GetTickets1();
    int jewels = GetJewels();

    // 총 기회 계산 (고급 티켓 + 쥬얼에 해당하는 뽑기 횟수)
    int totalPulls = tickets1 + (jewels / JEWEL_COST_PER_PULL);

    if (totalPulls == 0)
    {
        probabilityText.text = "사용할 티켓과 쥬얼을 입력해주세요.";
        return;
    }

    // 시뮬레이션을 몇 번 진행할지 결정 (1000번 정도가 적당)
    int simulationRuns = 1000;
    int pickupCount = 0;

    // 시뮬레이션 반복
    for (int i = 0; i < simulationRuns; i++)
    {
        List<string> results = PerformGacha(totalPulls, PICKUP_RATE, SSR_RATE, SR_RATE, R_RATE);

        // 픽업 캐릭터가 나왔는지 확인
        if (results.Exists(result => result.Contains("픽업 캐릭터")))
        {
            pickupCount++;
        }
    }

    // 확률 계산
    double probability = (pickupCount / (double)simulationRuns) * 100;

    // 결과를 텍스트로 출력
    probabilityText.text = $"가능한 {totalPulls}뽑 안에서\n{probability:F2}% 확률로 픽업이 등장했습니다.";
}

    List<string> PerformGacha(int pulls, double pickupRate, double ssrRate, double srRate, double rRate)
    {
        // 결과 저장용 리스트
        List<string> results = new List<string>();

        // 누적 확률 계산
        double[] cumulativeRates = {
            pickupRate,
            pickupRate + ssrRate,
            pickupRate + ssrRate + srRate,
            pickupRate + ssrRate + srRate + rRate
        };

        for (int i = 0; i < pulls; i++)
        {
            // 암호학적 난수 생성
            double roll = GenerateSecureRandom();

            // 결과 판별
            if (roll < cumulativeRates[0])
            {
                results.Add("픽업 캐릭터 (SSR)");
            }
            else if (roll < cumulativeRates[1])
            {
                results.Add("SSR 캐릭터");
            }
            else if (roll < cumulativeRates[2])
            {
                results.Add("SR 캐릭터");
            }
            else
            {
                results.Add("R 캐릭터");
            }
        // 티켓 사용 체크 (티켓이 부족하면 더 이상 사용되지 않음)
            if (ticketCount > 0)
            {
                ticketCount--;
            }
        }
        return results;
    }

    double GenerateSecureRandom()
    {
        // 0.0 ~ 1.0 범위의 암호학적 난수 생성
        byte[] randomBytes = new byte[8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // 64비트 정수로 변환 후 0.0 ~ 1.0 사이로 정규화
        ulong randomValue = BitConverter.ToUInt64(randomBytes, 0);
        return (double)randomValue / ulong.MaxValue;
    }

    // 티켓이 입력되지 않으면 0으로 설정, 입력값을 반환
    int GetTickets()
    {
        int tickets = 0;
        if (string.IsNullOrEmpty(ticketInputField.text) || !int.TryParse(ticketInputField.text, out tickets))
        {
            // 입력값이 비어있거나 잘못된 경우
            ticketInputField.text = "0";
        }
        return tickets;
    }

    int GetJewels(){
        int jewels = 0;
        if (string.IsNullOrEmpty(jewelInputField.text) || !int.TryParse(jewelInputField.text, out jewels)){
            jewelInputField.text = "0";
        }
        return jewels;
    }

    int GetTickets1()
    {
        int tickets1 = 0;
        if (string.IsNullOrEmpty(ticketInputField1.text) || !int.TryParse(ticketInputField1.text, out tickets1))
        {
            // 입력값이 비어있거나 잘못된 경우
            ticketInputField1.text = "0";
        }
        return tickets1;
    }

    void OnexpectButtonClicked(){
        int tickets = GetTickets();
        int pullsUntilPickup = SimulateGachaUntilPickup(ticketCount);

        // 티켓 사용은 픽업이 나오기 전에만
    int totalPulls = pullsUntilPickup;
    int usedPulls = Mathf.Min(totalPulls, tickets); // 티켓을 사용한 횟수
    int remainingPulls = totalPulls - usedPulls; // 티켓을 다 쓰고 난 후, 남은 뽑기 횟수

    // 소모된 쥬얼 계산 (티켓 사용은 쥬얼을 소모하지 않음)
    int jewelsUsed = remainingPulls * JEWEL_COST_PER_PULL;

    // 결과를 TMP 텍스트로 표시
    string resultTextOutput = $"픽업 캐릭터가 나오기까지 {pullsUntilPickup}뽑이 필요했습니다.\n";
    
        resultTextOutput += $"\n티켓: {usedPulls}장 사용";
    
    resultTextOutput += $"\n소모된 쥬얼: {jewelsUsed}";
    resultText.text = resultTextOutput;
    }


    // 픽업 캐릭터가 나올 때까지 반복 시행
    int SimulateGachaUntilPickup(int ticketCount)
    {
        int attempts = 0;
        bool pickupFound = false;

        while (!pickupFound)
        {
            attempts++;
            double roll = GenerateSecureRandom();

            // 픽업 캐릭터가 나왔을 경우
            if (roll < PICKUP_RATE)
            {
                pickupFound = true;
            }

            // 티켓 소모
            if (ticketCount > 0)
            {
                ticketCount--;
            }
        }

        return attempts; // 몇 번 만에 픽업 캐릭터가 나왔는지 반환
    }

    // 버튼 색상 변경 및 클릭 비활성화 코루틴
    IEnumerator ChangeButtonColorAndDisable(Color newColor, Button whut)
    {
        // 버튼 색상 변경
        whut.GetComponent<Image>().color = newColor;

        // 버튼 클릭 비활성화
        whut.interactable = false;

        // 색상 변화 이펙트 시간 (0.5초로 변경)
        yield return new WaitForSeconds(0.5f);

        // 버튼 색상 원래 상태로 돌아가기
        whut.GetComponent<Image>().color = Color.white;
        whut.interactable = true; // 클릭 가능하게 다시 활성화
    }

    // SSR이 없을 때는 다른 색을 반환하도록 하는 메서드
    Color GetRandomColor()
    {
        // 확률에 따라 색상 결정
        double roll = GenerateSecureRandom();
        if (roll < 0.002526)  // 0.2526% 확률로 파란색
        {
            return blueColor;
        }
        else  // 나머지 확률로 보라색
        {
            return purpleColor;
        }
    }
}