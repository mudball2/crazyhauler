using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   public void ExitButton() {
       Application.Quit();
       Debug.Log("Game closed.");
   }

   public void OptionsButton(){
       Debug.Log("Game Options pushed!");
   }

   public void CreditsButton(){
       SceneManager.LoadScene("credits");
   }

   public void StartGame() {
       SceneManager.LoadScene("Game");
   }
}
