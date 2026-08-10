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

    [Header("Animadores (Opcional para PoC)")]
    public Animator animJ1;
    public Animator animJ2;

    [Header("Placeholders de Armas (0:Espada, 1:Báculo, 2:Escudo)")]
    public GameObject[] armasJ1; // Arrastrar los 3 objetos hijos de J1 aquí
    public GameObject[] armasJ2; // Arrastrar los 3 objetos hijos de J2 aquí

    [Header("Efectos de Partículas (VFX)")]
    public ParticleSystem particulasMuertePrefab;
    public ParticleSystem particulasRespawnPrefab;

    [Header("Elecciones de Jugadores")]
    public OpcionCombate eleccionJ1 = OpcionCombate.Ninguna;
    public OpcionCombate eleccionJ2 = OpcionCombate.Ninguna;

    [Header("Estado del Turno")]
    public bool esperandoInput = false;
    public bool juegoTerminado = false;
    public float duracionMaxTurno = 4f;
    private float tiempoRestante;

    [Header("Progreso de Partida")]
    public int victoriasRondaJ1 = 0;
    public int victoriasRondaJ2 = 0;
    public int metaVictoriasRonda = 2;

    public int manaJ1 = 0;
    public int manaJ2 = 0;
    public int metaManaPartida = 3;

    [Header("UI - HUD de Juego")]
    public TextMeshProUGUI textoTemporizador;
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
        OcultarTodasLasArmas();
        ActualizarUI();
        if (panelVictoriaUI != null) panelVictoriaUI.SetActive(false);
        PrepararEsperarBoton();
    }

    void Update()
    {
        if (!esperandoInput || juegoTerminado) return;

        tiempoRestante -= Time.deltaTime;

        if (textoTemporizador != null)
        {
            int segundos = Mathf.CeilToInt(Mathf.Max(0, tiempoRestante));
            textoTemporizador.text = segundos.ToString();
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame) RegistrarEleccion(1, OpcionCombate.Espada);
            else if (Keyboard.current.sKey.wasPressedThisFrame) RegistrarEleccion(1, OpcionCombate.Baculo);
            else if (Keyboard.current.dKey.wasPressedThisFrame) RegistrarEleccion(1, OpcionCombate.Escudo);

            if (Keyboard.current.jKey.wasPressedThisFrame) RegistrarEleccion(2, OpcionCombate.Espada);
            else if (Keyboard.current.kKey.wasPressedThisFrame) RegistrarEleccion(2, OpcionCombate.Baculo);
            else if (Keyboard.current.lKey.wasPressedThisFrame) RegistrarEleccion(2, OpcionCombate.Escudo);
        }

        if (tiempoRestante <= 0f)
        {
            esperandoInput = false;
            Debug.Log("⏱️ ¡TIEMPO AGOTADO! Revelando armas...");
            ResolverTurno();
        }
    }

    public void PresionarBotonIniciarTurno()
    {
        if (juegoTerminado) return;
        if (botonIniciarTurnoUI != null) botonIniciarTurnoUI.SetActive(false);
        IniciarNuevoTurno();
    }

    private void PrepararEsperarBoton()
    {
        if (juegoTerminado) return;

        RestablecerPersonajesActivos();
        OcultarTodasLasArmas();

        esperandoInput = false;
        eleccionJ1 = OpcionCombate.Ninguna;
        eleccionJ2 = OpcionCombate.Ninguna;

        if (textoTemporizador != null) textoTemporizador.text = Mathf.CeilToInt(duracionMaxTurno).ToString();
        if (botonIniciarTurnoUI != null) botonIniciarTurnoUI.SetActive(true);
    }

    private void IniciarNuevoTurno()
    {
        eleccionJ1 = OpcionCombate.Ninguna;
        eleccionJ2 = OpcionCombate.Ninguna;
        tiempoRestante = duracionMaxTurno;
        esperandoInput = true;
    }

    private void RegistrarEleccion(int jugador, OpcionCombate eleccion)
    {
        if (jugador == 1) eleccionJ1 = eleccion;
        else if (jugador == 2) eleccionJ2 = eleccion;
    }

    private void OcultarTodasLasArmas()
    {
        foreach (var arma in armasJ1) if (arma != null) arma.SetActive(false);
        foreach (var arma in armasJ2) if (arma != null) arma.SetActive(false);
    }

    private void MostrarArma(int jugador, OpcionCombate eleccion)
    {
        GameObject[] arrayArmas = (jugador == 1) ? armasJ1 : armasJ2;
        int indice = -1;

        if (eleccion == OpcionCombate.Espada) indice = 0;
        else if (eleccion == OpcionCombate.Baculo) indice = 1;
        else if (eleccion == OpcionCombate.Escudo) indice = 2;

        if (indice >= 0 && indice < arrayArmas.Length && arrayArmas[indice] != null)
        {
            arrayArmas[indice].SetActive(true);
        }
    }

    private void ResolverTurno()
    {
        // Revelar visualmente las armas elegidas
        MostrarArma(1, eleccionJ1);
        MostrarArma(2, eleccionJ2);

        int ganadorTurno = 0; 

        if (eleccionJ1 == OpcionCombate.Ninguna && eleccionJ2 == OpcionCombate.Ninguna) ganadorTurno = 0;
        else if (eleccionJ1 == OpcionCombate.Ninguna) ganadorTurno = 2;
        else if (eleccionJ2 == OpcionCombate.Ninguna) ganadorTurno = 1;
        else if (eleccionJ1 == eleccionJ2)
        {
            Debug.Log($"⚡ ¡EMPATE!");
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

        // Disparar Animaciones de Victoria/Derrota
        if (ganadorTurno == 1)
        {
            if (animJ1 != null) animJ1.SetTrigger("Atacar");
            if (animJ2 != null) animJ2.SetTrigger("Morir");
            victoriasRondaJ1++;
        }
        else if (ganadorTurno == 2)
        {
            if (animJ2 != null) animJ2.SetTrigger("Atacar");
            if (animJ1 != null) animJ1.SetTrigger("Morir");
            victoriasRondaJ2++;
        }

        // Desactivar al perdedor con un pequeño retraso para ver la animación
        StartCoroutine(RutinaDesactivarPerdedor(1.0f, ganadorTurno));
    }

    private IEnumerator RutinaDesactivarPerdedor(float retraso, int ganadorTurno)
    {
        yield return new WaitForSeconds(retraso);

        if (ganadorTurno == 1) DesactivarPerdedor(2);
        else if (ganadorTurno == 2) DesactivarPerdedor(1);
        else if (ganadorTurno == 0) DesactivarPerdedor(0);

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
        else StartCoroutine(RutinaRespawnYNuevoTurno(2f));
    }

    // (El resto de las funciones: DesactivarPerdedor, RestablecerPersonajesActivos, GenerarEfectoVFX, EjecutarVictoriaGlobal, ReiniciarPartidaCompleta, ActualizarUI se mantienen exactamente iguales al script anterior)
    
    private void DesactivarPerdedor(int numeroPerdedor)
    {
        if (numeroPerdedor == 1 && jugador1GO != null)
        {
            GenerarEfectoVFX(particulasMuertePrefab, jugador1GO.transform.position);
            jugador1GO.SetActive(false);
        }
        else if (numeroPerdedor == 2 && jugador2GO != null)
        {
            GenerarEfectoVFX(particulasMuertePrefab, jugador2GO.transform.position);
            jugador2GO.SetActive(false);
        }
        else if (numeroPerdedor == 0)
        {
            if (jugador1GO != null) { GenerarEfectoVFX(particulasMuertePrefab, jugador1GO.transform.position); jugador1GO.SetActive(false); }
            if (jugador2GO != null) { GenerarEfectoVFX(particulasMuertePrefab, jugador2GO.transform.position); jugador2GO.SetActive(false); }
        }
    }

    private void RestablecerPersonajesActivos()
    {
        if (jugador1GO != null && !jugador1GO.activeSelf)
        {
            jugador1GO.SetActive(true);
            if (animJ1 != null) animJ1.SetTrigger("Renacer");
            GenerarEfectoVFX(particulasRespawnPrefab, jugador1GO.transform.position);
        }

        if (jugador2GO != null && !jugador2GO.activeSelf)
        {
            jugador2GO.SetActive(true);
            if (animJ2 != null) animJ2.SetTrigger("Renacer");
            GenerarEfectoVFX(particulasRespawnPrefab, jugador2GO.transform.position);
        }
    }

    private void GenerarEfectoVFX(ParticleSystem efectoPrefab, Vector3 posicion)
    {
        if (efectoPrefab != null)
        {
            ParticleSystem vfx = Instantiate(efectoPrefab, posicion, Quaternion.identity);
            vfx.Play();
            Destroy(vfx.gameObject, vfx.main.duration + vfx.main.startLifetime.constantMax);
        }
    }

    private void EjecutarVictoriaGlobal(int jugadorGanador)
    {
        juegoTerminado = true;
        esperandoInput = false;

        if (botonIniciarTurnoUI != null) botonIniciarTurnoUI.SetActive(false);
        if (panelVictoriaUI != null) panelVictoriaUI.SetActive(true);
        if (textoGanadorUI != null) textoGanadorUI.text = $"¡FATALITY!\n¡JUGADOR {jugadorGanador} ES LIBERADO!";
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
        PrepararEsperarBoton();
    }

    private IEnumerator RutinaRespawnYNuevoTurno(float tiempoEspera)
    {
        yield return new WaitForSeconds(tiempoEspera);
        PrepararEsperarBoton();
    }
}