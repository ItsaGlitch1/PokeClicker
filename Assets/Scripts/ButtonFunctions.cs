using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ButtonFunctions : MonoBehaviour
{
    bool StartShakeBall;
    bool FinishShakeBall;
    int BallCurrentRotationZ;
    float BallDeltaTime;
    bool isJumping;
    bool StarterHoverAnim;
    int StarterButton;
    float StarterDeltaTime;
    private void Start()
    {
        StartShakeBall = false;
        FinishShakeBall = false;
        BallCurrentRotationZ = 0;
        BallDeltaTime = 0;
    }
    private void Update()
    {
        if (StartShakeBall)
        {
            BallDeltaTime += Time.deltaTime;
            if (BallDeltaTime >= 1f/30f)
            {
                BallDeltaTime -= 1f/30f;
                if (!FinishShakeBall)
                {
                    GameManager.Instance.ThrownBall.transform.Rotate(0, 0, 8);
                    BallCurrentRotationZ += 8;
                    if (BallCurrentRotationZ > 20)
                        FinishShakeBall = true;
                }
                else
                {
                    GameManager.Instance.ThrownBall.transform.Rotate(0, 0, -8);
                    BallCurrentRotationZ -= 8;
                    if (BallCurrentRotationZ < 1)
                    {
                        StartShakeBall = false;
                        FinishShakeBall = false;
                    }

                }
            }
        }
        if (StarterHoverAnim)
        {
            StarterDeltaTime += Time.deltaTime;
            if (StarterDeltaTime > 0.3f)
            {
                StarterDeltaTime -= 0.3f;
                Vector3 spritePos = GameManager.Instance.StarterNames[StarterButton].transform.parent.localPosition;
                Vector3 textPos = GameManager.Instance.StarterNames[StarterButton].transform.localPosition;
                if (!isJumping)
                {
                    spritePos.y += 15;
                    textPos.y -= 15;
                    isJumping = true;
                }
                else
                {
                    spritePos.y -= 15;
                    textPos.y += 15;
                    isJumping = false;
                }
                GameManager.Instance.StarterNames[StarterButton].transform.parent.localPosition = spritePos;
                GameManager.Instance.StarterNames[StarterButton].transform.localPosition = textPos;
            }
        }
    }
    public void NewStarter(int dexID)
    {
        if (dexID > 151)
        {
            foreach (Pokemon pokemon in PokemonList.OtherStarters)
            {
                if (pokemon.dexID == dexID && pokemon.reginalForm == RegionalForm.None)
                {
                    Player.Instance.SelectStarter(new Pokemon(pokemon, 5));
                }
            }
        }
        else
        {
            Player.Instance.SelectStarter(new Pokemon(PokemonList.PokemonData[dexID], 5));
        }
        
        ClickerButtonScript.Instance.newPokemon();
        GameManager.Instance.StarterSelection.SetActive(false);
        EnemyAI.Instance.PauseAttack(false);
    }
    public void NewHisuianStarter(int dexID)
    {
        foreach (Pokemon pokemon in PokemonList.OtherStarters)
        {
            if (pokemon.dexID == dexID && pokemon.reginalForm == RegionalForm.Hisuian)
            {
                Player.Instance.SelectStarter(new Pokemon(pokemon, 5));
            }
        }
        ClickerButtonScript.Instance.newPokemon();
        GameManager.Instance.StarterSelection.SetActive(false);
        EnemyAI.Instance.PauseAttack(false);
    }
    public void ThrowBall(int BallID)
    {
        BallType ball = PokemonList.BallTypes[BallID];
        GameManager.Instance.ThrowBall.interactable = false;
        GameManager.Instance.EnemySprite.enabled = false;
        GameManager.Instance.ThrownBall.enabled = true;
        EnemyAI.Instance.PauseAttack(true);
        Pokemon pokemon = ClickerButtonScript.Instance.enemy;
        
        //Shakes on Successes
        //float CatchChance = Mathf.Floor(((3 * pokemon.maxHP - 2 * pokemon.currHP) / 3 * pokemon.maxHP) * 4096 + 0.5f);
        //CatchChance *= 250; //Species Catch Rate
        //CatchChance *= ball.BaseCatchRate;
        ////CatchChance *= StatusCondition;
        //float ShakeChance = 65535 / (float)Math.Pow(CatchChance/1044480, 0.1875);
        StartCoroutine(BallShakes(0.8f));
        
    }
    IEnumerator BallShakes(float ShakeChance)
    {
        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(1);
            bool success = UnityEngine.Random.Range(0f, 1f) < ShakeChance;
            if (!success)
            {
                //Break out
                GameManager.Instance.EnemySprite.enabled = true;
                GameManager.Instance.ThrownBall.enabled = false;
                GameManager.Instance.ThrowBall.interactable = true;
                EnemyAI.Instance.PauseAttack(false);
                EnemyAI.Instance.Attack();
                yield break;
            }
            if (i != 3)
            {
                StartShakeBall = true;
            }

        }
        GameManager.Instance.ThrownBall.color = Color.gray;
        yield return new WaitForSeconds(1);
        Player.Instance.AddToParty(new Pokemon(ClickerButtonScript.Instance.enemy));
        GameManager.Instance.IncreaseStageEnemiesDefeated();
        ClickerButtonScript.Instance.newPokemon();
        GameManager.Instance.ThrownBall.color = Color.white;
        GameManager.Instance.EnemySprite.enabled = true;
        GameManager.Instance.ThrownBall.enabled = false;
        EnemyAI.Instance.PauseAttack(false);
        GameManager.Instance.ThrowBall.interactable = true;
    }
    public void SetActivePartyPokemon(int slot)
    {
        if (Player.Instance.party[slot] != null && Player.Instance.party[slot].currHP > 0)
        {
            Player.Instance.SetActivePokemon(slot);
        }
    }


    public void Restart()
    {
        Player.Instance.takingDamage = false;
        ClickerButtonScript.Instance.takingDamage = false;
        Player.Instance.party.Clear();
        int i = 0;
        foreach (UnityEngine.UI.Button partyButton in GameManager.Instance.PartySlots)
        {
            partyButton.interactable = false;
            GameManager.Instance.ClearPartySlot(i);
            ++i;
        }
        GameManager.Instance.setStage(1);
        GameManager.Instance.StageEnemiesDefeated = 0;
        GameManager.Instance.StarterSelection.SetActive(true);
        GameManager.Instance.GameOverScreen.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void ToggleCredits(bool toggle)
    {
        GameManager.Instance.CreditsScreen.SetActive(toggle);
        if (toggle)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }
    public void StarterScreenOnHover(int button)
    {
        StarterDeltaTime = 0.3f;
        StarterButton = button;
        isJumping = false;
        StarterHoverAnim = true;
        GameManager.Instance.StarterNames[button].fontStyle = TMPro.FontStyles.Bold;
        //bool
    }
    public void StarterScreenEndHover(int button)
    {
        StarterHoverAnim = false;
        GameManager.Instance.StarterNames[button].fontStyle = TMPro.FontStyles.Normal;
        if (isJumping)
        {
            Vector3 spritePos = GameManager.Instance.StarterNames[button].transform.parent.localPosition;
            Vector3 textPos = GameManager.Instance.StarterNames[button].transform.localPosition;
            spritePos.y -= 15;
            textPos.y += 15;
            GameManager.Instance.StarterNames[button].transform.parent.localPosition = spritePos;
            GameManager.Instance.StarterNames[button].transform.localPosition = textPos;
            isJumping = false;
        }
    }
}
