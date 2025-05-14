import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import './Dashboard.css';

const Dashboard = () => {
    const [weeksData, setWeeksData] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const response = await axios.get('/api/dengue/ultimas-3-semanas');
                setWeeksData(response.data);
            } catch (err) {
                setError(err.response?.data?.message || err.message);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, []);

    const getAlertColor = (level) => {
        switch (level) {
            case 1: return '#4CAF50'; // Verde
            case 2: return '#FFEB3B'; // Amarelo
            case 3: return '#FF9800'; // Laranja
            case 4: return '#F44336'; // Vermelho
            default: return '#9E9E9E'; // Cinza
        }
    };

    if (loading) return <div className="loading-spinner">Carregando dados...</div>;
    if (error) return <div className="error-message">Erro: {error}</div>;

    return (
        <div className="dashboard-container">
            <h1>Monitoramento de Dengue - Belo Horizonte</h1>
            <h2>Últimas 3 semanas epidemiológicas</h2>

            {/* Tabela */}
            <div className="table-container">
                <table>
                    <thead>
                        <tr>
                            <th>Semana Epidemiológica</th>
                            <th>Casos Estimados</th>
                            <th>Casos Notificados</th>
                            <th>Nível de Alerta</th>
                        </tr>
                    </thead>
                    <tbody>
                        {weeksData.map((week, index) => (
                            <tr key={index}>
                                <td>{week.semanaEpidemiologica}</td>
                                <td>{week.casosEstimados}</td>
                                <td>{week.casosNotificados}</td>
                                <td style={{ backgroundColor: getAlertColor(week.nivelAlerta) }}>
                                    {week.nivelAlerta}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {/* Gráfico */}
            <div className="chart-container">
                <h3>Evolução dos Casos</h3>
                <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={weeksData}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="semanaEpidemiologica" />
                        <YAxis />
                        <Tooltip />
                        <Legend />
                        <Bar dataKey="casosEstimados" name="Casos Estimados" fill="#8884d8" />
                        <Bar dataKey="casosNotificados" name="Casos Notificados" fill="#82ca9d" />
                    </BarChart>
                </ResponsiveContainer>
            </div>

            {/* Cards */}
            <div className="cards-container">
                {weeksData.map((week, index) => (
                    <div
                        key={index}
                        className="week-card"
                        style={{ borderColor: getAlertColor(week.nivelAlerta) }}
                    >
                        <h3>Semana {week.semanaEpidemiologica}</h3>
                        <div className="card-row">
                            <span>Casos Estimados:</span>
                            <strong>{week.casosEstimados}</strong>
                        </div>
                        <div className="card-row">
                            <span>Casos Notificados:</span>
                            <strong>{week.casosNotificados}</strong>
                        </div>
                        <div className="card-row">
                            <span>Nível de Alerta:</span>
                            <strong style={{ color: getAlertColor(week.nivelAlerta) }}>
                                {week.nivelAlerta}
                            </strong>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default Dashboard;