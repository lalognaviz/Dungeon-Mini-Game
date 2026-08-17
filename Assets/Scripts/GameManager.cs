using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum OpcionCombate { Ninguna, Espada, Baculo, Escudo }

    [Header("Objetos de los Jugadores")]
    public GameObject jugador1GO;
    public GameObject jugador2GO;

    [Header("Animadores")]
    public Animator animJ1;
    public Animator animJ2;

    [Header("Audio - Sistema de Turno y SFX")]
    public AudioSource musicaTurnoSource;
    public AudioSource sfxSource;
    public AudioClip musicaTurnoClip;
    public AudioClip sfxAtaque;
    public AudioClip sfxMuerte;
    public AudioClip sfxBoton;

    [Header("Configuración de Duración Dinámica")]
    public float duracionMinimaTurno = 4f;
    public float duracionMaximaTurno = 8f;
    private float tiempoRestanteTurno;

    [Header("Elecciones de Jugadores")]
    public OpcionCombate eleccionJ1 = OpcionCombate.Ninguna;
    public OpcionCombate eleccionJ2 = OpcionCombate.Ninguna;

    [Header("Estado del Turno")]
    public bool esperandoInput = false;
    public bool juegoTerminado = false;

    [Header("Progreso de Partida")]
    public int victoriasRondaJ1 = 0;
    public int victoriasRondaJ2 = 0;
    public int metaVictoriasRonda = 2;

    public int manaJ1 = 0;
    public int manaJ2 = 0;
    public int metaManaPartida = 3;

    [Header("UI - HUD de Juego")]
    public TextMeshProUGUI textoRondaJ1;
    public TextMeshProUGUI textoRondaJ2;
    public TextMeshProUGUI textoManaJ1;
    public TextMeshProUGUI textoManaJ2;
    public GameObject botonIniciarTurnoUI;

    [Header("UI - Pantalla de Victoria")]
    public GameObject panelVictoriaUI;
    public TextMeshProUGUI textoGanadorUI;

    void Start()
    {
        ActualizarUI();
        if (panelVictoriaUI != null) panelVictoriaUI.SetActive(false);
        RestablecerAmbosAIdle();
        PrepararEsperarBoton();
    }

    void Update()
    {
        if (!esperandoInput || juegoTerminado) return;

        tiempoRestanteTurno -= Time.deltaTime;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame) RegistrarEleccion(1, OpcionCombate.Espada);
            else if (Keyboard.current.sKey.wasPressedThisFrame) RegistrarEleccion(1, OpcionCombate.Baculo);
            else if (Keyboard.current.dKey.wasPressedThisFrame) RegistrarEleccion(1, OpcionCombate.Escudo);

            if (Keyboard.current.jKey.wasPressedThisFrame) RegistrarEleccion(2, OpcionCombate.Espada);
            else if (Keyboard.current.kKey.wasPressedThisFrame) RegistrarEleccion(2, OpcionCombate.Baculo);
            else if (Keyboard.current.lKey.wasPressedThisFrame) RegistrarEleccion(2, OpcionCombate.Escudo);
        }

        if (tiempoRestanteTurno <= 0f)
        {
            esperandoInput = false;
            if (musicaTurnoSource != null) musicaTurnoSource.Stop();
            Debug.Log("⏱️ ¡Fin del turno aleatorio! Revelando combate...");
            ResolverTurno();
        }
    }

    public void PresionarBotonIniciarTurno()
    {
        if (juegoTerminado) return;
        ReproducirSFX(sfxBoton);
        if (botonIniciarTurnoUI != null) botonIniciarTurnoUI.SetActive(false);
        IniciarNuevoTurno();
    }

    private void PrepararEsperarBoton()
    {
        if (juegoTerminado) return;

        esperandoInput = false;
        eleccionJ1 = OpcionCombate.Ninguna;
        eleccionJ2 = OpcionCombate.Ninguna;

        if (botonIniciarTurnoUI != null) botonIniciarTurnoUI.SetActive(true);
    }

    private void IniciarNuevoTurno()
    {
        RestablecerAmbosAIdle();

        eleccionJ1 = OpcionCombate.Ninguna;
        eleccionJ2 = OpcionCombate.Ninguna;

        float duracionElegida = Random.Range(duracionMinimaTurno, duracionMaximaTurno);
        tiempoRestanteTurno = duracionElegida;
        esperandoInput = true;

        if (musicaTurnoSource != null && musicaTurnoClip != null)
        {
            musicaTurnoSource.clip = musicaTurnoClip;
            musicaTurnoSource.loop = false;
            musicaTurnoSource.pitch = 1f; // Aseguramos que el audio siempre suene a velocidad normal
            musicaTurnoSource.Play();
        }

        Debug.Log($"--- ⚔️ TURNO EN CURSO | Duración: {duracionElegida:F2}s ⚔️ ---");
    }

    private void RegistrarEleccion(int jugador, OpcionCombate eleccion)
    {
        if (jugador == 1)
        {
            if (eleccionJ1 != eleccion)
            {
                eleccionJ1 = eleccion;
                ReproducirAnimacion(animJ1, "Desenvaina", eleccion);
            }
        }
        else if (jugador == 2)
        {
            if (eleccionJ2 != eleccion)
            {
                eleccionJ2 = eleccion;
                ReproducirAnimacion(animJ2, "Desenvaina", eleccion);
            }
        }
    }

    private void ReproducirAnimacion(Animator anim, string accion, OpcionCombate arma)
    {
        if (anim == null) return;
        string estadoAnimacion = "";

        if (accion == "Atacar")
        {
            if (arma == OpcionCombate.Espada) estadoAnimacion = "Ataque_Espada";
            else if (arma == OpcionCombate.Baculo) estadoAnimacion = "Ataque_Baculo";
            else if (arma == OpcionCombate.Escudo) estadoAnimacion = "Ataque_Escudo";
        }
        else if (accion == "Morir")
        {
            if (arma == OpcionCombate.Espada) estadoAnimacion = "Morir_Espada";
            else if (arma == OpcionCombate.Baculo) estadoAnimacion = "Morir_Baculo";
            else if (arma == OpcionCombate.Escudo) estadoAnimacion = "Morir_Escudo";
        }
        else if (accion == "Desenvaina")
        {
            if (arma == OpcionCombate.Espada) estadoAnimacion = "Desenvaina_Espada";
            else if (arma == OpcionCombate.Baculo) estadoAnimacion = "Desenvaina_Baculo";
            else if (arma == OpcionCombate.Escudo) estadoAnimacion = "Desenvaina_Escudo";
        }

        if (estadoAnimacion != "")
        {
            anim.Play(estadoAnimacion, 0, 0f);
        }
    }

    private void ResolverTurno()
    {
        int ganadorTurno = 0; 

        if (eleccionJ1 == OpcionCombate.Ninguna && eleccionJ2 == OpcionCombate.Ninguna) ganadorTurno = 0;
        else if (eleccionJ1 == OpcionCombate.Ninguna) ganadorTurno = 2;
        else if (eleccionJ2 == OpcionCombate.Ninguna) ganadorTurno = 1;
        else if (eleccionJ1 == eleccionJ2)
        {
            Debug.Log("⚡ ¡EMPATE!");
            StartCoroutine(RutinaReiniciarTurno(1.5f));
            return;
        }
        else
        {
            bool ganaJ1 = (eleccionJ1 == OpcionCombate.Escudo && eleccionJ2 == OpcionCombate.Espada) ||
                          (eleccionJ1 == OpcionCombate.Espada && eleccionJ2 == OpcionCombate.Baculo) ||
                          (eleccionJ1 == OpcionCombate.Baculo && eleccionJ2 == OpcionCombate.Escudo);
            ganadorTurno = ganaJ1 ? 1 : 2;
        }

        if (ganadorTurno == 1)
        {
            ReproducirAnimacion(animJ1, "Atacar", eleccionJ1);
            ReproducirAnimacion(animJ2, "Morir", eleccionJ2);
            ReproducirSFX(sfxAtaque);
            victoriasRondaJ1++;
        }
        else if (ganadorTurno == 2)
        {
            ReproducirAnimacion(animJ2, "Atacar", eleccionJ2);
            ReproducirAnimacion(animJ1, "Morir", eleccionJ1);
            ReproducirSFX(sfxAtaque);
            victoriasRondaJ2++;
        }
        else
        {
            ReproducirSFX(sfxMuerte);
        }

        EvaluarRondaYMana();
    }

    private void EvaluarRondaYMana()
    {
        if (victoriasRondaJ1 >= metaVictoriasRonda)
        {
            manaJ1++;
            ReiniciarContadoresRonda();
        }
        else if (victoriasRondaJ2 >= metaVictoriasRonda)
        {
            manaJ2++;
            ReiniciarContadoresRonda();
        }

        ActualizarUI();

        if (manaJ1 >= metaManaPartida) EjecutarVictoriaGlobal(1);
        else if (manaJ2 >= metaManaPartida) EjecutarVictoriaGlobal(2);
        else PrepararEsperarBoton();
    }

    private void RestablecerAmbosAIdle()
    {
        if (animJ1 != null) animJ1.Play("Idle", 0, 0f);
        if (animJ2 != null) animJ2.Play("Idle", 0, 0f);
    }

    private void ReproducirSFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(clip);
        }
    }

    private void EjecutarVictoriaGlobal(int jugadorGanador)
    {
        juegoTerminado = true;
        esperandoInput = false;

        if (musicaTurnoSource != null) musicaTurnoSource.Stop();
        if (botonIniciarTurnoUI != null) botonIniciarTurnoUI.SetActive(false);
        if (panelVictoriaUI != null) panelVictoriaUI.SetActive(true);

        if (textoGanadorUI != null)
        {
            textoGanadorUI.text = $"¡FATALITY!\n¡JUGADOR {jugadorGanador} ES LIBERADO!";
        }
    }

    public void ReiniciarPartidaCompleta()
    {
        manaJ1 = 0;
        manaJ2 = 0;
        victoriasRondaJ1 = 0;
        victoriasRondaJ2 = 0;
        juegoTerminado = false;

        if (panelVictoriaUI != null) panelVictoriaUI.SetActive(false);

        ActualizarUI();
        RestablecerAmbosAIdle();
        PrepararEsperarBoton();
    }

    private void ReiniciarContadoresRonda()
    {
        victoriasRondaJ1 = 0;
        victoriasRondaJ2 = 0;
    }

    private void ActualizarUI()
    {
        if (textoRondaJ1 != null) textoRondaJ1.text = $"Ronda J1: {victoriasRondaJ1}/{metaVictoriasRonda}";
        if (textoRondaJ2 != null) textoRondaJ2.text = $"Ronda J2: {victoriasRondaJ2}/{metaVictoriasRonda}";
        if (textoManaJ1 != null) textoManaJ1.text = $"Maná J1: {manaJ1}/{metaManaPartida}";
        if (textoManaJ2 != null) textoManaJ2.text = $"Maná J2: {manaJ2}/{metaManaPartida}";
    }

    private IEnumerator RutinaReiniciarTurno(float tiempoEspera)
    {
        yield return new WaitForSeconds(tiempoEspera);
        RestablecerAmbosAIdle();
        PrepararEsperarBoton();
    }
}