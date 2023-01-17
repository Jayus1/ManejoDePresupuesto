function inicializarformulariotransacciones(urlObtenerCategoria)
{
    $("#TipoOperacionId").change(async function () {
        const valorSeleccionado = $(this).val();

        const respuesta = await fetch('', {
            method: 'POST',
            body: valorSeleccionado,
            headers: {
                'Content-Type': 'application/jsn'
            }
        });

        const json = await respuesta.json();
        const opciones = json.map(categoria => `<option.value=${categoria.value}> ${categoria.text} </option>`);
        $("#CategoriaId").html(opciones);
    })
}