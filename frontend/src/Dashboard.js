import React, { useEffect, useState } from 'react';

function Dashboard() {
    const [dados, setDados] = useState([]);
    const [carregando, setCarregando] = useState(true);
    const [erro, setErro] = useState(null);

    useEffect(() => {
        fetch('/api/dengue/ultimas-semanas')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Erro ao buscar dados da API');
                }
                return response.json();
            })
            .then(data => {
                setDados(data);
                setCarregando(false);
            })
            .catch(error => {
                setErro(error.message);
                setCarregando(false);
            });
    }, []);

    if (carregando) return <p>Carregando dados...</p>;
    if (erro) return <p style={{ color: 'red' }}>Erro: {erro}</p>;

    return (
        <div style={{ padding: '20px' }}>
            <h2>Casos de Dengue - Últimas Semanas</h2>
            <table border="1" cellPadding="8">
                <thead>
                    <tr>
                        <th>Semana Epidemiológica</th>
                        <th>Casos Estimados</th>
                        <th>Casos Notificados</th>
                        <th>Nível de Alerta</th>
                    </tr>
                </thead>
                <tbody>
                    {dados.map((item, index) => (
                        <tr key={index}>
                            <td>{item.semanaEpidemiologica}</td>
                            <td>{item.casosEstimados}</td>
                            <td>{item.casosNotificados}</td>
                            <td style={{ color: item.corNivelAlerta }}>
                                {item.nivelAlerta}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}

export default Dashboard;
